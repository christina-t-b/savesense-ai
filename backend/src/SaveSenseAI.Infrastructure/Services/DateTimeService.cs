using SaveSenseAI.Application.Common.Interfaces;

namespace SaveSenseAI.Infrastructure.Services;

public sealed class DateTimeService : IDateTime
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
