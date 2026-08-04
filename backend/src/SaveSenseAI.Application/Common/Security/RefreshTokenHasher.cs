using System.Security.Cryptography;
using System.Text;

namespace SaveSenseAI.Application.Common.Security;

/// <summary>
/// Hashes raw refresh tokens for storage/lookup. SHA-256 (not a slow
/// password hash like BCrypt/Argon2) is correct here — the input is a
/// 64-byte cryptographically random string, not a human-guessable password,
/// so there's no brute-force risk to slow down against.
/// </summary>
public static class RefreshTokenHasher
{
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
