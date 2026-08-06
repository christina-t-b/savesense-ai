using MediatR;

namespace SaveSenseAI.Application.Auth.Commands.LoginWithGoogle;

/// <summary>
/// Takes already-verified claims from Google, not a raw token — the API
/// layer's Google authentication handler does the actual OAuth code
/// exchange and signature verification before this command ever runs.
/// </summary>
public sealed record LoginWithGoogleCommand(string GoogleSubjectId, string Email, string DisplayName) : IRequest<AuthTokensResult>;
