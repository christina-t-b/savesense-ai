using MediatR;
using SaveSenseAI.Application.Health.Queries.GetPing;

namespace SaveSenseAI.API.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health");

        group.MapGet("/ping", async (ISender sender, string? name, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetPingQuery(name), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetPing")
        .Produces<PingResult>();

        return app;
    }
}
