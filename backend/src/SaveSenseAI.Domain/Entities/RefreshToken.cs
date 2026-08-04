using SaveSenseAI.Domain.Common;

namespace SaveSenseAI.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }

    /// <summary>
    /// SHA-256 hash of the raw token. The raw value is only ever returned to
    /// the caller once, at issuance — if the database leaked, a stolen hash
    /// can't be replayed as a token.
    /// </summary>
    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Set when this token was rotated out for a new one, so a reused
    /// (already-rotated) token can be detected and the whole chain revoked.
    /// </summary>
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive(DateTimeOffset nowUtc) => !IsRevoked && !IsExpired(nowUtc);

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset nowUtc, TimeSpan lifetime)
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = nowUtc.Add(lifetime),
        };
    }

    public void Revoke(DateTimeOffset nowUtc, Guid? replacedByTokenId = null)
    {
        RevokedAtUtc = nowUtc;
        ReplacedByTokenId = replacedByTokenId;
    }
}
