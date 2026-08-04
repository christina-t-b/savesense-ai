using SaveSenseAI.Domain.Common;

namespace SaveSenseAI.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;

    /// <summary>
    /// Google's stable per-account identifier (the ID token's "sub" claim).
    /// Used to find the same user across logins instead of trusting email,
    /// which a provider could theoretically let change.
    /// </summary>
    public string GoogleSubjectId { get; private set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    private User() { }

    public static User CreateFromGoogle(string email, string displayName, string googleSubjectId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(googleSubjectId))
        {
            throw new ArgumentException("Google subject id is required.", nameof(googleSubjectId));
        }

        return new User
        {
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? email : displayName,
            GoogleSubjectId = googleSubjectId,
            CreatedAtUtc = nowUtc,
        };
    }
}
