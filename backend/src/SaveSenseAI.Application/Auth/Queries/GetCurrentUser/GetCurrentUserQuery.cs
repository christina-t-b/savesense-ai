using MediatR;

namespace SaveSenseAI.Application.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserResult>;

public sealed record CurrentUserResult(Guid Id, string Email, string DisplayName);
