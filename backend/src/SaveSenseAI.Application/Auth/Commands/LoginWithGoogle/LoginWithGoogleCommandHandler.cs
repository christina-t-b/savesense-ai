using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SaveSenseAI.Application.Common;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Application.Common.Security;
using SaveSenseAI.Domain.Entities;

namespace SaveSenseAI.Application.Auth.Commands.LoginWithGoogle;

public sealed class LoginWithGoogleCommandHandler(
    IApplicationDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IDateTime dateTime,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<LoginWithGoogleCommand, AuthTokensResult>
{
    private readonly JwtSettings _settings = jwtSettings.Value;

    public async Task<AuthTokensResult> Handle(LoginWithGoogleCommand request, CancellationToken cancellationToken)
    {
        var now = dateTime.UtcNow;

        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.GoogleSubjectId == request.GoogleSubjectId, cancellationToken);

        if (user is null)
        {
            user = User.CreateFromGoogle(request.Email, request.DisplayName, request.GoogleSubjectId, now);
            dbContext.Users.Add(user);
        }
        else
        {
            user.UpdateProfile(request.Email, request.DisplayName);
        }

        var rawRefreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(
            user.Id,
            RefreshTokenHasher.Hash(rawRefreshToken),
            now,
            TimeSpan.FromDays(_settings.RefreshTokenExpiryDays));

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = jwtTokenService.GenerateAccessToken(user);

        return new AuthTokensResult(
            accessToken,
            now.AddMinutes(_settings.AccessTokenExpiryMinutes),
            rawRefreshToken,
            refreshToken.ExpiresAtUtc);
    }
}
