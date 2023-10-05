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
            var createTicketResponse = await _ticketingOptions.TicketingService.CreateTicket(new Dictionary<string, string>(), cancellationToken);
            // var getAllResponse = await _ticketingOptions.TicketingService.GetAll(cancellationToken);
            var getResponse = await _ticketingOptions.TicketingService.Get(createTicketResponse, cancellationToken);
            await _ticketingOptions.TicketingService.Pending(createTicketResponse, cancellationToken);
            await _ticketingOptions.TicketingService.Error(createTicketResponse, new TicketError("HealthCheck error call", ""), cancellationToken);
            await _ticketingOptions.TicketingService.Complete(createTicketResponse, new TicketResult(), cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
