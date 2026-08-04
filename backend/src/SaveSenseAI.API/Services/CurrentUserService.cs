using System.IdentityModel.Tokens.Jwt;
using SaveSenseAI.Application.Common.Interfaces;

namespace SaveSenseAI.API.Services;

/// <summary>
/// Reads the authenticated user's id from the current request's claims.
/// Lives in API, not Infrastructure — HttpContext is a web-layer concept,
/// and API is the one project already coupled to ASP.NET Core's web SDK.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
