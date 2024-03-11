namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using BackOffice.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Options;
using TicketingService.Abstractions;
using TicketingService.Proxy.HttpProxy;

public static class TicketingExtensions
{
    public static IServiceCollection AddTicketing(this IServiceCollection services)
    {
        return services
            .RegisterOptions<TicketingOptions>()
            .AddSingleton<ITicketingUrl>(sp =>
                {
                    var ticketingOptions = sp.GetRequiredService<TicketingOptions>();
                    return new TicketingUrl(ticketingOptions.InternalBaseUrl);
                }
            )
            .AddHttpProxyTicketing()
        ;
    }
    
    private static IServiceCollection AddHttpProxyTicketing(this IServiceCollection services)
    {
        services.AddHttpClient<ITicketing, HttpProxyTicketing>((sp, c) =>
        {
            var ticketingOptions = sp.GetRequiredService<TicketingOptions>();
            c.BaseAddress = new Uri(ticketingOptions.InternalBaseUrl.TrimEnd('/'));
        });

        return services;
    }
}
