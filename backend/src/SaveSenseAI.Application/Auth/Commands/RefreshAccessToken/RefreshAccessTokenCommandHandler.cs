using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaveSenseAI.Application.Common;
using SaveSenseAI.Application.Common.Exceptions;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Application.Common.Security;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Auth.Commands.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler(
    IApplicationDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IDateTime dateTime,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<RefreshAccessTokenCommand, AuthTokensResult>
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public async Task<AuthTokensResult> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var now = dateTime.UtcNow;
        var incomingHash = RefreshTokenHasher.Hash(request.RefreshToken);

        var existingToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == incomingHash, cancellationToken);

        if (existingToken is null)
        {
            throw new AuthenticationFailedException("Invalid refresh token.");
        }

        if (existingToken.IsRevoked)
        {
            // A token that was already rotated out is being presented again —
            // that means it (or a copy of it) was stolen. Kill every active
            // token for this user so the thief's session dies too, not just
            // this one request.
            await RevokeAllActiveTokensForUserAsync(existingToken.UserId, now, cancellationToken);
            throw new AuthenticationFailedException("Refresh token has already been used.");
        }

        if (existingToken.IsExpired(now))
        {
            throw new AuthenticationFailedException("Refresh token has expired.");
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == existingToken.UserId, cancellationToken)
            ?? throw new AuthenticationFailedException("User no longer exists.");

        var newRawRefreshToken = jwtTokenService.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(
            user.Id,
            RefreshTokenHasher.Hash(newRawRefreshToken),
            now,
            TimeSpan.FromDays(_settings.RefreshTokenExpiryDays));

        dbContext.RefreshTokens.Add(newRefreshToken);
        existingToken.Revoke(now, newRefreshToken.Id);

        await dbContext.SaveChangesAsync(cancellationToken);

        var newAccessToken = jwtTokenService.GenerateAccessToken(user);

        return new AuthTokensResult(
            newAccessToken,
            newRawRefreshToken,
            now.AddMinutes(_settings.AccessTokenExpiryMinutes));
    }

    private async Task RevokeAllActiveTokensForUserAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var activeTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(now);
        }

        if (activeTokens.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
