namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using TicketingService.Abstractions;
using TicketingService.Proxy.HttpProxy;

public static class TicketingExtensions
{
    private static IServiceCollection AddHttpProxyTicketing(this IServiceCollection services)
    {
        services.AddHttpClient<ITicketing, HttpProxyTicketing>((sp, c) =>
        {
            var options = sp.GetRequiredService<TicketingOptions>();
            c.BaseAddress = new Uri(options.InternalBaseUrl.TrimEnd('/'));
        });

        return services;
    }

    public static IServiceCollection AddTicketing(
        this IServiceCollection services, IConfiguration configuration)
    {
        var ticketingOptions = new TicketingOptions();
        configuration.GetSection(TicketingOptions.ConfigurationKey).Bind(ticketingOptions);

        return services
                .AddSingleton(ticketingOptions)
                .AddHttpProxyTicketing()
                .AddSingleton<ITicketingUrl>(sp =>
                    new TicketingUrl(ticketingOptions.InternalBaseUrl)
                )
            ;
    }
}
