using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Auth.Commands.Logout;
using SaveSenseAI.Application.Common.Security;
using SaveSenseAI.Application.UnitTests.Common;
using SaveSenseAI.Domain.Entities;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Application.UnitTests.Auth.Commands.Logout;

public class LogoutCommandHandlerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesIt()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var user = User.CreateFromGoogle("christina@example.com", "Christina", "sub-1", now);
        var rawToken = "raw-refresh-token";
        var token = RefreshToken.Create(user.Id, RefreshTokenHasher.Hash(rawToken), now, TimeSpan.FromDays(30));
        context.Users.Add(user);
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LogoutCommandHandler(context, new FixedDateTime(now));
        await handler.Handle(new LogoutCommand(rawToken), CancellationToken.None);

        var persisted = await context.RefreshTokens.SingleAsync(rt => rt.Id == token.Id);
        Assert.True(persisted.IsRevoked);
    }

    [Fact]
    public async Task Handle_UnknownToken_DoesNotThrow()
    {
        await using var context = CreateContext();
        var handler = new LogoutCommandHandler(context, new FixedDateTime(DateTimeOffset.UtcNow));

        var exception = await Record.ExceptionAsync(
            () => handler.Handle(new LogoutCommand("never-issued"), CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Handle_AlreadyRevokedToken_DoesNotThrowOrChangeRevocationTime()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var user = User.CreateFromGoogle("christina@example.com", "Christina", "sub-1", now);
        var rawToken = "raw-refresh-token";
        var token = RefreshToken.Create(user.Id, RefreshTokenHasher.Hash(rawToken), now.AddDays(-1), TimeSpan.FromDays(30));
        var firstRevocation = now.AddHours(-1);
        token.Revoke(firstRevocation);
        context.Users.Add(user);
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new LogoutCommandHandler(context, new FixedDateTime(now));
        await handler.Handle(new LogoutCommand(rawToken), CancellationToken.None);

        var persisted = await context.RefreshTokens.SingleAsync(rt => rt.Id == token.Id);
        Assert.Equal(firstRevocation, persisted.RevokedAtUtc);
    }
}
