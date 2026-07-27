using MediatR;

namespace SaveSenseAI.Application.Health.Queries.GetPing;

/// <summary>
/// Proves the CQRS pipeline end-to-end (API -> MediatR -> Validation ->
/// Handler) before any real feature exists. Intentionally trivial.
/// </summary>
public sealed record GetPingQuery(string? Name) : IRequest<PingResult>;

public sealed record PingResult(string Message, DateTimeOffset ServerTimeUtc);
