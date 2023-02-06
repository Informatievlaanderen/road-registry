namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Options;
using TicketingService.Abstractions;
using TicketingService.Proxy.HttpProxy;

public static class TicketingExtensions
{
    private static IServiceCollection AddHttpProxyTicketing(this IServiceCollection services, TicketingOptions ticketingOptions)
    {
        services.AddHttpClient<ITicketing, HttpProxyTicketing>((sp, c) =>
        {
            c.BaseAddress = new Uri(ticketingOptions.InternalBaseUrl.TrimEnd('/'));
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
                .AddHttpProxyTicketing(ticketingOptions)
                .AddSingleton<ITicketingUrl>(sp =>
                    new TicketingUrl(ticketingOptions.InternalBaseUrl)
                )
            ;
    }
}
