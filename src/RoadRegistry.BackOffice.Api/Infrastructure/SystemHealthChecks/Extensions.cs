namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthChecks;

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddSystemHealthChecks(this IServiceCollection services)
    {
        var systemHealthCheckTypes = GetHealthCheckTypes();

        services.AddSingleton<ISystemHealthCheckService, SystemHealthCheckService>();
        services.AddSingleton(new SystemHealthCheckOptions
        {
            HealthCheckTypes = systemHealthCheckTypes
        });

        foreach (var systemHealthCheckType in systemHealthCheckTypes)
        {
            services.AddScoped(systemHealthCheckType);
        }

        return services;
    }
    
    private static Type[] GetHealthCheckTypes()
    {
        var interfaceType = typeof(ISystemHealthCheck);

        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsAbstract)
            .ToArray();
    }
}
