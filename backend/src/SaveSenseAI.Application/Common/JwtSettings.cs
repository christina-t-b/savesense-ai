namespace SaveSenseAI.Application.Common;

/// <summary>
/// Bound from the "Jwt" configuration section. SigningKey is only ever
/// supplied via dotnet user-secrets (dev) or environment variables/Key Vault
/// (prod) — never committed to appsettings.json.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string SigningKey { get; init; } = null!;
    public int AccessTokenExpiryMinutes { get; init; } = 15;
    public int RefreshTokenExpiryDays { get; init; } = 30;
}
