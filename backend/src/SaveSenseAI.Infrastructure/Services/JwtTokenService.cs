using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaveSenseAI.Application.Common;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtSettings, IDateTime dateTime) : IJwtTokenService
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public string GenerateAccessToken(User user)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Standard short claim names (sub/email/name), not ClaimTypes.* URIs —
        // matches how a real JWT looks. The API layer's JwtBearerOptions
        // (Step 3) must set MapInboundClaims = false and NameClaimType =
        // JwtRegisteredClaimNames.Sub, otherwise ASP.NET Core silently
        // rewrites "sub" back into the long ClaimTypes.NameIdentifier URI on
        // the way in — a well-known .NET JWT gotcha.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: dateTime.UtcNow.UtcDateTime,
            expires: dateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
