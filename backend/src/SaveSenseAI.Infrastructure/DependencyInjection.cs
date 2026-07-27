using Microsoft.Extensions.DependencyInjection;
using SaveSenseAI.Application.Common.Interfaces;
using SaveSenseAI.Infrastructure.Services;

namespace SaveSenseAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTime, DateTimeService>();

        return services;
    }
}
