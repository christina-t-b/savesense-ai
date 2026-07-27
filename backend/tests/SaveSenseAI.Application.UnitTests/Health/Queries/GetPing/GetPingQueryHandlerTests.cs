using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Application.Health.Queries.GetPing;

namespace SaveSenseAI.Application.UnitTests.Health.Queries.GetPing;

public class GetPingQueryHandlerTests
{
    private sealed class FixedDateTime(DateTimeOffset now) : IDateTime
    {
        public DateTimeOffset UtcNow => now;
    }

    [Fact]
    public async Task Handle_WithName_ReturnsGreetingAndCurrentTime()
    {
        var fixedNow = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var handler = new GetPingQueryHandler(new FixedDateTime(fixedNow));

        var result = await handler.Handle(new GetPingQuery("Christina"), CancellationToken.None);

        Assert.Equal("pong, Christina", result.Message);
        Assert.Equal(fixedNow, result.ServerTimeUtc);
    }

    [Fact]
    public async Task Handle_WithoutName_DefaultsToWorld()
    {
        var handler = new GetPingQueryHandler(new FixedDateTime(DateTimeOffset.UtcNow));

        var result = await handler.Handle(new GetPingQuery(null), CancellationToken.None);

        Assert.Equal("pong, world", result.Message);
    }
}
