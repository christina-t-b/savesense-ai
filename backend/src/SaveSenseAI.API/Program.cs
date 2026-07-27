using SaveSenseAI.API.Endpoints;
using SaveSenseAI.API.Middleware;
using SaveSenseAI.Application;
using SaveSenseAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthEndpoints();

app.Run();

// Exposed for WebApplicationFactory-based integration tests in later phases.
public partial class Program;
