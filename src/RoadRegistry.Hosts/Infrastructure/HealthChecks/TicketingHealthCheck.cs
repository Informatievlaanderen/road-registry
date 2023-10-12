namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Options;
using TicketingService.Abstractions;

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
            await _ticketingOptions.TicketingService.Get(ticketId, cancellationToken);
            await _ticketingOptions.TicketingService.Pending(ticketId, cancellationToken);
            await _ticketingOptions.TicketingService.Error(ticketId, new TicketError("HealthCheck error call", ""), cancellationToken);
            await _ticketingOptions.TicketingService.Complete(ticketId, new TicketResult(), cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
