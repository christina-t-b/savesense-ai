using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Common.Interfaces;

public interface IJwtTokenService
{
    /// <summary>Short-lived signed JWT identifying the user, sent on every API request.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Long-lived opaque random string. Never a JWT — it carries no
    /// claims, it's just a lookup key for the RefreshTokens table.</summary>
    string GenerateRefreshToken();
}
