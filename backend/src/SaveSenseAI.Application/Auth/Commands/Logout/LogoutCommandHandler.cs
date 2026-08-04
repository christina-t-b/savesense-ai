using MediatR;
using Microsoft.EntityFrameworkCore;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Application.Common.Security;

namespace SaveSenseAI.Application.Auth.Commands.Logout;

/// <summary>
/// Deliberately doesn't distinguish "token didn't exist" from "token
/// revoked" in its outcome — both look like success to the caller, so a
/// forged/expired token can't be used to probe for valid-looking ones.
/// </summary>
public sealed class LogoutCommandHandler(IApplicationDbContext dbContext, IDateTime dateTime)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = RefreshTokenHasher.Hash(request.RefreshToken);
        var token = await dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);

        if (token is not null && !token.IsRevoked)
        {
            token.Revoke(dateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
