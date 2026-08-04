using MediatR;

namespace SaveSenseAI.Application.Auth.Commands.RefreshAccessToken;

public sealed record RefreshAccessTokenCommand(string RefreshToken) : IRequest<AuthTokensResult>;
