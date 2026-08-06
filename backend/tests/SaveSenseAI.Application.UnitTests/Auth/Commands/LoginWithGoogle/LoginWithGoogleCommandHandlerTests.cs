using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaveSenseAI.Application.Auth.Commands.LoginWithGoogle;
using SaveSenseAI.Application.Common;
using SaveSenseAI.Application.UnitTests.Common;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Application.UnitTests.Auth.Commands.LoginWithGoogle;

public class LoginWithGoogleCommandHandlerTests
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
    public async Task Handle_NewGoogleSubject_CreatesUserAndIssuesTokens()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var handler = new LoginWithGoogleCommandHandler(context, new FakeJwtTokenService(), new FixedDateTime(now), Settings());

        var result = await handler.Handle(
            new LoginWithGoogleCommand("google-sub-1", "christina@example.com", "Christina"), CancellationToken.None);

        var user = await context.Users.SingleAsync(u => u.GoogleSubjectId == "google-sub-1");
        Assert.Equal("christina@example.com", user.Email);
        Assert.Equal($"access-token-for-{user.Id}", result.AccessToken);

        var persistedToken = await context.RefreshTokens.SingleAsync(rt => rt.UserId == user.Id);
        Assert.False(persistedToken.IsRevoked);
    }

    [Fact]
    public async Task Handle_ExistingGoogleSubject_ReusesUserAndSyncsProfile()
    {
        await using var context = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var firstLoginHandler = new LoginWithGoogleCommandHandler(context, new FakeJwtTokenService(), new FixedDateTime(now), Settings());
        await firstLoginHandler.Handle(
            new LoginWithGoogleCommand("google-sub-1", "old-email@example.com", "Old Name"), CancellationToken.None);

        var secondLoginHandler = new LoginWithGoogleCommandHandler(context, new FakeJwtTokenService(), new FixedDateTime(now), Settings());
        await secondLoginHandler.Handle(
            new LoginWithGoogleCommand("google-sub-1", "new-email@example.com", "New Name"), CancellationToken.None);

        Assert.Equal(1, await context.Users.CountAsync());
        var user = await context.Users.SingleAsync();
        Assert.Equal("new-email@example.com", user.Email);
        Assert.Equal("New Name", user.DisplayName);
        Assert.Equal(2, await context.RefreshTokens.CountAsync(rt => rt.UserId == user.Id));
    }
}
