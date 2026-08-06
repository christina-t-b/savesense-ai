using MediatR;
using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Common.Exceptions;
using SaveSenseAI.Application.Common.Interfaces;

namespace SaveSenseAI.Application.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResult>
{
    public async Task<CurrentUserResult> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // currentUserService.UserId being null here would mean this ran
        // without RequireAuthorization() ever having checked a valid JWT —
        // an API-layer wiring bug, not a legitimate "no user" case.
        var userId = currentUserService.UserId
            ?? throw new AuthenticationFailedException("No authenticated user.");

        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new AuthenticationFailedException("User no longer exists.");

        return new CurrentUserResult(user.Id, user.Email, user.DisplayName);
    }
}
