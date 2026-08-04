using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaveSenseAI.Application.Auth.Commands.RefreshAccessToken;
using SaveSenseAI.Application.Common;
using SaveSenseAI.Application.Common.Exceptions;
using SaveSenseAI.Application.Common.Security;
using SaveSenseAI.Application.UnitTests.Common;
using SaveSenseAI.Domain.Entities;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Application.UnitTests.Auth.Commands.RefreshAccessToken;

public class RefreshAccessTokenCommandHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static IOptions<JwtSettings> Settings() => Options.Create(new JwtSettings
    {
        Issuer = "test",
        Audience = "test",
        SigningKey = "unused-in-these-tests",
        AccessTokenExpiryMinutes = 15,
        RefreshTokenExpiryDays = 30,
    });

    [Fact]
    public async Task Handle_ValidActiveToken_RotatesAndReturnsNewTokens()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var user = User.CreateFromGoogle("christina@example.com", "Christina", "sub-1", now);
        var rawToken = "raw-refresh-token";
        var oldToken = SaveSenseAI.Domain.Entities.RefreshToken.Create(user.Id, RefreshTokenHasher.Hash(rawToken), now, TimeSpan.FromDays(30));
        context.Users.Add(user);
        context.RefreshTokens.Add(oldToken);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RefreshAccessTokenCommandHandler(context, new FakeJwtTokenService(), new FixedDateTime(now), Settings());

        var result = await handler.Handle(new RefreshAccessTokenCommand(rawToken), CancellationToken.None);

        Assert.Equal($"access-token-for-{user.Id}", result.AccessToken);
        Assert.NotEqual(rawToken, result.RefreshToken);

        var persistedOldToken = await context.RefreshTokens.SingleAsync(rt => rt.Id == oldToken.Id);
        Assert.True(persistedOldToken.IsRevoked);
        Assert.NotNull(persistedOldToken.ReplacedByTokenId);

        var newToken = await context.RefreshTokens.SingleAsync(rt => rt.Id == persistedOldToken.ReplacedByTokenId);
        Assert.False(newToken.IsRevoked);
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsAuthenticationFailed()
    {
        await using var context = CreateContext();
        var handler = new RefreshAccessTokenCommandHandler(
            context, new FakeJwtTokenService(), new FixedDateTime(DateTimeOffset.UtcNow), Settings());

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => handler.Handle(new RefreshAccessTokenCommand("never-issued"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsAuthenticationFailed()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var user = User.CreateFromGoogle("christina@example.com", "Christina", "sub-1", now);
        var rawToken = "raw-refresh-token";
        var expiredToken = SaveSenseAI.Domain.Entities.RefreshToken.Create(
            user.Id, RefreshTokenHasher.Hash(rawToken), now.AddDays(-31), TimeSpan.FromDays(30));
        context.Users.Add(user);
        context.RefreshTokens.Add(expiredToken);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RefreshAccessTokenCommandHandler(context, new FakeJwtTokenService(), new FixedDateTime(now), Settings());

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => handler.Handle(new RefreshAccessTokenCommand(rawToken), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReusedRevokedToken_ThrowsAndRevokesAllActiveTokensForUser()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var user = User.CreateFromGoogle("christina@example.com", "Christina", "sub-1", now);
        context.Users.Add(user);

        var reusedRawToken = "already-rotated-token";
        var reusedToken = SaveSenseAI.Domain.Entities.RefreshToken.Create(
            user.Id, RefreshTokenHasher.Hash(reusedRawToken), now.AddDays(-1), TimeSpan.FromDays(30));
        reusedToken.Revoke(now.AddHours(-1));

        var otherActiveToken = SaveSenseAI.Domain.Entities.RefreshToken.Create(
            user.Id, RefreshTokenHasher.Hash("some-other-active-token"), now, TimeSpan.FromDays(30));

        context.RefreshTokens.AddRange(reusedToken, otherActiveToken);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new RefreshAccessTokenCommandHandler(context, new FakeJwtTokenService(), new FixedDateTime(now), Settings());

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => handler.Handle(new RefreshAccessTokenCommand(reusedRawToken), CancellationToken.None));

        var persistedOtherToken = await context.RefreshTokens.SingleAsync(rt => rt.Id == otherActiveToken.Id);
        Assert.True(persistedOtherToken.IsRevoked);
    }
}
