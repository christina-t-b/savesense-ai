using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.UnitTests.Common;

public sealed class FixedDateTime(DateTimeOffset now) : IDateTime
{
    public DateTimeOffset UtcNow { get; set; } = now;
}

/// <summary>Deterministic stand-in for JwtTokenService — real signing is
/// covered by SaveSenseAI.Infrastructure.UnitTests; these handler tests only
/// need to know a "new token" was requested and what came back.</summary>
public sealed class FakeJwtTokenService : IJwtTokenService
{
    private int _refreshTokenCounter;

    public string GenerateAccessToken(User user) => $"access-token-for-{user.Id}";

    public string GenerateRefreshToken() => $"refresh-token-{Interlocked.Increment(ref _refreshTokenCounter)}";
}

public sealed class FakeCurrentUserService(Guid? userId = null) : ICurrentUserService
{
    public Guid? UserId { get; set; } = userId;
}
