using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.IntegrationTests.Persistence;

public class ApplicationDbContextTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    private static string UniqueEmail() => $"{Guid.NewGuid()}@example.com";

    [Fact]
    public async Task Users_CanBeInsertedAndRetrieved()
    {
        await using var context = fixture.CreateContext();
        var email = UniqueEmail();
        var user = User.CreateFromGoogle(email, "Christina", Guid.NewGuid().ToString(), DateTimeOffset.UtcNow);

        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        await using var readContext = fixture.CreateContext();
        var retrieved = await readContext.Users.SingleAsync(u => u.Id == user.Id);

        Assert.Equal(email, retrieved.Email);
        Assert.Equal("Christina", retrieved.DisplayName);
    }

    [Fact]
    public async Task Users_DuplicateEmail_ViolatesUniqueConstraint()
    {
        await using var context = fixture.CreateContext();
        var email = UniqueEmail();
        var googleSubjectId = Guid.NewGuid().ToString();

        context.Users.Add(User.CreateFromGoogle(email, "First", googleSubjectId, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync(CancellationToken.None);

        await using var secondContext = fixture.CreateContext();
        secondContext.Users.Add(User.CreateFromGoogle(email, "Second", Guid.NewGuid().ToString(), DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() => secondContext.SaveChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task DeletingUser_CascadesToRefreshTokens()
    {
        await using var context = fixture.CreateContext();
        var user = User.CreateFromGoogle(UniqueEmail(), "Christina", Guid.NewGuid().ToString(), DateTimeOffset.UtcNow);
        context.Users.Add(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var token = RefreshToken.Create(user.Id, "hash-value", DateTimeOffset.UtcNow, TimeSpan.FromDays(30));
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync(CancellationToken.None);

        await using var deleteContext = fixture.CreateContext();
        var userToDelete = await deleteContext.Users.SingleAsync(u => u.Id == user.Id);
        deleteContext.Users.Remove(userToDelete);
        await deleteContext.SaveChangesAsync(CancellationToken.None);

        await using var verifyContext = fixture.CreateContext();
        var remainingToken = await verifyContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Id == token.Id);
        Assert.Null(remainingToken);
    }
}
