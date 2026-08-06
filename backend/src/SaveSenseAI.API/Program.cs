using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SaveSenseAI.API.Auth;
using SaveSenseAI.API.Endpoints;
using SaveSenseAI.API.Middleware;
using SaveSenseAI.API.Services;
using SaveSenseAI.Application;
using SaveSenseAI.Application.Auth.Commands.LoginWithGoogle;
using SaveSenseAI.Application.Common;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "Frontend";
const string ExternalCookieScheme = "External";

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");
var frontendBaseUrl = builder.Configuration["Frontend:BaseUrl"]
    ?? throw new InvalidOperationException("Frontend:BaseUrl configuration is missing.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<AuthenticationFailedExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

builder.Services
    .AddAuthentication(options =>
    {
        // JWT bearer is what protects normal API endpoints — a missing/bad
        // token means a clean 401, not a redirect to Google. Google is only
        // ever triggered explicitly, by the /api/auth/google/login endpoint.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = ExternalCookieScheme;
    })
    .AddJwtBearer(options =>
    {
        // Keep claim types exactly as issued ("sub", not the ClaimTypes.*
        // URI ASP.NET Core remaps them to by default) — CurrentUserService
        // reads JwtRegisteredClaimNames.Sub directly, matching JwtTokenService.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    })
    .AddCookie(ExternalCookieScheme, options =>
    {
        // Only ever holds Google's claims for the few seconds between the
        // OAuth redirect and OnTicketReceived below — never a real session.
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.Cookie.Name = "external_login";
    })
    .AddGoogle(options =>
    {
        options.SignInScheme = ExternalCookieScheme;
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Authentication:Google:ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException("Authentication:Google:ClientSecret is not configured.");
        options.CallbackPath = "/signin-google";

        // Intercept right after Google's claims are verified, issue our own
        // tokens, and skip ASP.NET Core's default "sign in with the external
        // cookie and redirect to ReturnUrl" behavior entirely — the browser
        // never carries a Google token past this point.
        options.Events.OnTicketReceived = async context =>
        {
            var principal = context.Principal!;
            var googleSubjectId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var email = principal.FindFirstValue(ClaimTypes.Email)!;
            var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? email;

            var sender = context.HttpContext.RequestServices.GetRequiredService<ISender>();
            var result = await sender.Send(new LoginWithGoogleCommand(googleSubjectId, email, displayName));

            RefreshTokenCookie.Append(context.HttpContext.Response, result.RefreshToken, result.RefreshTokenExpiresAtUtc);

            context.HttpContext.Response.Redirect($"{frontendBaseUrl}/auth/callback");
            context.HandleResponse();
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints();

app.Run();

// Exposed for WebApplicationFactory-based integration tests in later phases.
public partial class Program;
