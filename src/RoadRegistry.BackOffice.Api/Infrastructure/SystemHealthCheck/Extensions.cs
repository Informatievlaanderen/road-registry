namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TicketingService.Abstractions;

internal static class Extensions
{
    public static IServiceCollection AddSystemHealthChecks(this IServiceCollection services, ICollection<Type> systemHealthCheckTypes)
    {
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

    public static async Task<HealthCheckResult> WaitUntilCompleteOrTimeout(this ITicketing ticketing, Guid ticketId, TimeSpan timeoutAt, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        while (true)
        {
            if (sw.Elapsed > timeoutAt)
            {
                return HealthCheckResult.Unhealthy($"Timed out while waiting for ticket ({ticketId}) to complete at {sw.Elapsed}");
            }

            var ticket = await ticketing.Get(ticketId, cancellationToken);

            if (ticket != null)
            {
                if (ticket.Status == TicketStatus.Complete)
                {
                    return HealthCheckResult.Healthy();
                }

                if (ticket.Status == TicketStatus.Error)
                {
                    return HealthCheckResult.Unhealthy($"Ticket ({ticketId}) resulted in error: {ticket.Result?.ResultAsJson}");
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
