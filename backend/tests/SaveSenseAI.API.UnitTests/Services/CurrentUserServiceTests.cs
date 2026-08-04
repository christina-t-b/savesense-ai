using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SaveSenseAI.API.Services;

namespace SaveSenseAI.API.UnitTests.Services;

public class CurrentUserServiceTests
{
    private static IHttpContextAccessor AccessorFor(HttpContext? context)
    {
        var accessor = new HttpContextAccessor { HttpContext = context };
        return accessor;
    }

    [Fact]
    public void UserId_WithValidSubClaim_ReturnsParsedGuid()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]);
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var service = new CurrentUserService(AccessorFor(context));

        Assert.Equal(userId, service.UserId);
    }

    [Fact]
    public void UserId_WithNoAuthenticatedUser_ReturnsNull()
    {
        var context = new DefaultHttpContext();

        var service = new CurrentUserService(AccessorFor(context));

        Assert.Null(service.UserId);
    }

    [Fact]
    public void UserId_WithNoHttpContext_ReturnsNull()
    {
        var service = new CurrentUserService(AccessorFor(null));

        Assert.Null(service.UserId);
    }

    [Fact]
    public void UserId_WithMalformedSubClaim_ReturnsNull()
    {
        var identity = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "not-a-guid")]);
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        var service = new CurrentUserService(AccessorFor(context));

        Assert.Null(service.UserId);
    }
}
