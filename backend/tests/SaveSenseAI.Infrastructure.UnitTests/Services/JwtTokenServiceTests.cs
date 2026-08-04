using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaveSenseAI.Application.Common;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Domain.Entities;
using SaveSenseAI.Infrastructure.Services;

namespace SaveSenseAI.Infrastructure.UnitTests.Services;

public class JwtTokenServiceTests
{
    private const string SigningKey = "this-is-a-test-signing-key-at-least-32-bytes-long";

    private sealed class FixedDateTime(DateTimeOffset now) : IDateTime
    {
        public DateTimeOffset UtcNow => now;
    }

    private static JwtTokenService CreateService(DateTimeOffset now) =>
        new(Options.Create(new JwtSettings
        {
            Issuer = "https://test-issuer",
            Audience = "test-audience",
            SigningKey = SigningKey,
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 30,
        }), new FixedDateTime(now));

    [Fact]
    public void GenerateAccessToken_ProducesTokenWithExpectedClaimsAndExpiry()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var service = CreateService(now);
        var user = User.CreateFromGoogle("christina@example.com", "Christina", "google-sub-123", now);

        var token = service.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("christina@example.com", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("https://test-issuer", jwt.Issuer);
        Assert.Equal("test-audience", jwt.Audiences.Single());
        Assert.Equal(now.AddMinutes(15).UtcDateTime, jwt.ValidTo);
    }

    [Fact]
    public void GenerateAccessToken_IsSignedWithTheConfiguredKey()
    {
        var now = DateTimeOffset.UtcNow;
        var service = CreateService(now);
        var user = User.CreateFromGoogle("christina@example.com", "Christina", "google-sub-123", now);

        var token = service.GenerateAccessToken(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = "https://test-issuer",
            ValidAudience = "test-audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
        };

        // Throws if the signature doesn't match — this is the actual proof
        // the token is trustworthy, not just well-formed.
        new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
    }

    [Fact]
    public void GenerateRefreshToken_ProducesDifferentValuesEachCall()
    {
        var service = CreateService(DateTimeOffset.UtcNow);

        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();

        Assert.NotEqual(first, second);
    }
}
