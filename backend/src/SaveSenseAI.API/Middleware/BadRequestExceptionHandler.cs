using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SaveSenseAI.API.Middleware;

/// <summary>
/// Minimal API parameter binding throws BadHttpRequestException for
/// malformed JSON, wrong types, etc. Without this, it falls through to the
/// generic ProblemDetails fallback as a 500 — technically a client error,
/// masquerading as a server one. Caught this via real testing, not by
/// inspection: a bad enum value in a request body surfaced as a 500.
/// </summary>
public sealed class BadRequestExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequestException)
        {
            return false;
        }

        httpContext.Response.StatusCode = badRequestException.StatusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = badRequestException.StatusCode,
                Title = "The request could not be understood.",
                Detail = badRequestException.Message,
            },
        });
    }
}
