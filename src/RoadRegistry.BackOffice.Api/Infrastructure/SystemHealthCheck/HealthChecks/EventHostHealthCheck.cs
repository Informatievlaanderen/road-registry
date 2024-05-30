namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck.HealthChecks;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using Messages;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqlStreamStore.Streams;
using TicketingService.Abstractions;

internal class EventHostHealthCheck : ISystemHealthCheck
{
    private readonly ITicketing _ticketing;
    private readonly IRoadNetworkEventWriter _roadNetworkEventWriter;

    public EventHostHealthCheck(
        ITicketing ticketing,
        IRoadNetworkEventWriter roadNetworkEventWriter)
    {
        _ticketing = ticketing;
        _roadNetworkEventWriter = roadNetworkEventWriter;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var ticketId = await _ticketing.CreateTicket(new Dictionary<string, string>
            {
                { "Action", "HealthCheck"}
            }, cancellationToken);

        await _roadNetworkEventWriter.WriteAsync(
            new StreamName("healthcheck"),
            ExpectedVersion.Any,
            new Event(new EventHostSystemHealthCheckRequested
            {
                TicketId = ticketId
            }),
            cancellationToken);

        return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, TimeSpan.FromMinutes(1), cancellationToken);
    }
}
