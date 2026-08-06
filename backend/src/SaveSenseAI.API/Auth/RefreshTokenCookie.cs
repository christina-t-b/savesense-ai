namespace SaveSenseAI.API.Auth;

/// <summary>
/// The refresh token never touches browser JS — httpOnly means script can't
/// read it, Secure means it's only sent over HTTPS (localhost is exempted
/// by browsers), SameSite=Lax means it rides along on the top-level
/// redirect back from Google but not on cross-site POSTs from elsewhere.
/// </summary>
public static class RefreshTokenCookie
{
    public const string Name = "refresh_token";

    public static void Append(HttpResponse response, string rawToken, DateTimeOffset expiresAtUtc)
    {
        response.Cookies.Append(Name, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAtUtc,
            Path = "/api/auth",
        });
    }

    public static void Delete(HttpResponse response)
    {
        response.Cookies.Delete(Name, new CookieOptions { Path = "/api/auth" });
    }
}
