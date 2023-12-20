namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class TicketingHealthCheck : IHealthCheck
{
    private readonly TicketingHealthCheckOptions _ticketingOptions;

    public TicketingHealthCheck(TicketingHealthCheckOptions ticketingOptions)
    {
        _ticketingOptions = ticketingOptions;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var ticketId = await _ticketingOptions.TicketingService.CreateTicket(new Dictionary<string, string>(), cancellationToken);
            await _ticketingOptions.TicketingService.Delete(ticketId, cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
