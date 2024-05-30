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

internal class ExtractHostHealthCheck : ISystemHealthCheck
{
    private readonly ITicketing _ticketing;
    private readonly IRoadNetworkEventWriter _roadNetworkExtractWriter;

    public ExtractHostHealthCheck(
        ITicketing ticketing,
        IRoadNetworkEventWriter roadNetworkExtractWriter)
    {
        _ticketing = ticketing;
        _roadNetworkExtractWriter = roadNetworkExtractWriter;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var ticketId = await _ticketing.CreateTicket(new Dictionary<string, string>
        {
            { "Action", "HealthCheck"}
        }, cancellationToken);

        await _roadNetworkExtractWriter.WriteAsync(
            new StreamName("healthcheck"),
            ExpectedVersion.Any,
            new Event(new ExtractHostSystemHealthCheckRequested
            {
                TicketId = ticketId
            }),
            cancellationToken);

        return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, TimeSpan.FromMinutes(1), cancellationToken);
    }
}
