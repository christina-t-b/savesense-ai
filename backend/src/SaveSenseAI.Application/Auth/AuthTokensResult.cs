namespace SaveSenseAI.Application.Auth;

public sealed record AuthTokensResult(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAtUtc);
