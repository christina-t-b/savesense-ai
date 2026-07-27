using MediatR;
using SaveSenseAI.Application.Common.Interfaces;

namespace SaveSenseAI.Application.Health.Queries.GetPing;

public sealed class GetPingQueryHandler(IDateTime dateTime) : IRequestHandler<GetPingQuery, PingResult>
{
    public Task<PingResult> Handle(GetPingQuery request, CancellationToken cancellationToken)
    {
        var name = string.IsNullOrWhiteSpace(request.Name) ? "world" : request.Name.Trim();
        return Task.FromResult(new PingResult($"pong, {name}", dateTime.UtcNow));
    }
}
