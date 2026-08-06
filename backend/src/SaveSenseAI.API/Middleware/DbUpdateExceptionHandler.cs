using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SaveSenseAI.API.Middleware;

/// <summary>
/// A unique-constraint violation (duplicate slug, duplicate coupon code per
/// store, etc.) is a client error — the request conflicts with existing
/// data — not a server failure. Deliberately doesn't try to parse which
/// constraint failed from the provider-specific inner exception; that's
/// coupling the API layer to Postgres error message formats for a marginal
/// gain. A generic 409 is honest about what's known without being fragile.
/// </summary>
public sealed class DbUpdateExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DbUpdateException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "The request conflicts with existing data.",
                Detail = "A record with one of these values already exists.",
            },
        });
    }
}
