using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SaveSenseAI.API.Middleware;

/// <summary>
/// Translates a FluentValidation.ValidationException thrown by the
/// Application layer's pipeline behavior into an RFC 7807 ProblemDetails
/// 400 response, instead of letting it fall through as an unhandled 500.
/// </summary>
public sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            },
        });
    }
}
