using MediatR;
using Microsoft.AspNetCore.Authentication.Google;
using SaveSenseAI.API.Auth;
using SaveSenseAI.Application.Auth.Commands.Logout;
using SaveSenseAI.Application.Auth.Commands.RefreshAccessToken;
using SaveSenseAI.Application.Auth.Queries.GetCurrentUser;
using SaveSenseAI.Application.Common.Exceptions;

namespace SaveSenseAI.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapGet("/google/login", () =>
            Results.Challenge(authenticationSchemes: [GoogleDefaults.AuthenticationScheme]));

        group.MapPost("/refresh", async (HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
        {
            var rawToken = httpContext.Request.Cookies[RefreshTokenCookie.Name];
            if (string.IsNullOrEmpty(rawToken))
            {
                throw new AuthenticationFailedException("No refresh token was supplied.");
            }

            var result = await sender.Send(new RefreshAccessTokenCommand(rawToken), cancellationToken);
            RefreshTokenCookie.Append(httpContext.Response, result.RefreshToken, result.RefreshTokenExpiresAtUtc);

            return Results.Ok(new { accessToken = result.AccessToken, expiresAtUtc = result.AccessTokenExpiresAtUtc });
        });

        group.MapPost("/logout", async (HttpContext httpContext, ISender sender, CancellationToken cancellationToken) =>
        {
            var rawToken = httpContext.Request.Cookies[RefreshTokenCookie.Name];
            if (!string.IsNullOrEmpty(rawToken))
            {
                await sender.Send(new LogoutCommand(rawToken), cancellationToken);
            }

            RefreshTokenCookie.Delete(httpContext.Response);
            return Results.NoContent();
        });

        group.MapGet("/me", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization();

        return app;
    }
}
