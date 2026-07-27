namespace SaveSenseAI.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the system clock. Handlers depend on this instead of
/// calling DateTimeOffset.UtcNow directly, so tests can supply a fixed time
/// without needing to sleep or tolerate flaky assertions.
/// </summary>
public interface IDateTime
{
    DateTimeOffset UtcNow { get; }
}
