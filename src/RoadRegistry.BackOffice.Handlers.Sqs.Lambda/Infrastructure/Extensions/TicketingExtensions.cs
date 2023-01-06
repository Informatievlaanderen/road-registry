using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Options;
using TicketingService.Abstractions;
using TicketingService.Proxy.HttpProxy;

public static class TicketingExtensions
{
    private static string GetBaseUrl(IConfiguration configuration) =>
        configuration.GetSection(TicketingOptions.ConfigurationKey).GetRequiredValue<string>(nameof(TicketingOptions.InternalBaseUrl));
    
    public static IServiceCollection AddTicketing(
        this IServiceCollection services)
    {
        return services
            .AddHttpProxyTicketing(GetBaseUrl)
            ;
    }

    private static IServiceCollection AddHttpProxyTicketing(
        this IServiceCollection services,
        Func<IConfiguration, string> baseUrlProvider)
    {
        services.AddHttpClient<ITicketing, HttpProxyTicketing>((sp, c) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            c.BaseAddress = new Uri(baseUrlProvider(configuration).TrimEnd('/'));
        });

        return services;
    }
}
