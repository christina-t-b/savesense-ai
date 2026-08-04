using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Infrastructure.Persistence;

namespace SaveSenseAI.Infrastructure.IntegrationTests.Persistence;

/// <summary>
/// Points at a real Postgres database and applies migrations once per test
/// run. Requires a reachable Postgres — set via the
/// INTEGRATION_TEST_DB_CONNECTION environment variable, or falls back to the
/// same local dev database docker-compose/Homebrew Postgres provisions.
/// A CI environment (Phase 10) supplies this via a Postgres service
/// container, the same way any real deployment would.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("INTEGRATION_TEST_DB_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=savesenseai_test;Username=savesenseai;Password=savesenseai_dev_password";

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
