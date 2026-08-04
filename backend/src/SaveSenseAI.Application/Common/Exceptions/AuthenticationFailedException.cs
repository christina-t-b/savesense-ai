namespace SaveSenseAI.Application.Common.Exceptions;

/// <summary>
/// Thrown when a refresh token is missing, expired, revoked, or reused.
/// Mapped to HTTP 401 by the API layer (Step 3) — never a validation error.
/// </summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);
