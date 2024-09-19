namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck.HealthChecks;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Uploads;
using Framework;
using Messages;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqlStreamStore.Streams;
using TicketingService.Abstractions;

internal class EventHostSystemHealthCheck : ISystemHealthCheck
{
    private readonly ITicketing _ticketing;
    private readonly IRoadNetworkEventWriter _roadNetworkEventWriter;
    private readonly RoadNetworkUploadsBlobClient _roadNetworkUploadsBlobClient;

    public EventHostSystemHealthCheck(
        ITicketing ticketing,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        RoadNetworkUploadsBlobClient roadNetworkUploadsBlobClient)
    {
        _ticketing = ticketing;
        _roadNetworkEventWriter = roadNetworkEventWriter;
        _roadNetworkUploadsBlobClient = roadNetworkUploadsBlobClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var ticketId = await _ticketing.CreateTicket(new Dictionary<string, string>
            {
                { "Action", "HealthCheck"}
            }, cancellationToken);

        var fileName = "healthcheck-eventhost.bin";
        Task.WaitAll(
            _roadNetworkUploadsBlobClient.CreateDummyFile(fileName, cancellationToken)
        );

        await _roadNetworkEventWriter.WriteAsync(
            new StreamName("healthcheck"),
            ExpectedVersion.Any,
            new Event(new EventHostSystemHealthCheckRequested
            {
                TicketId = ticketId,
                BucketFileName = fileName
            }),
            cancellationToken);

        return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, TimeSpan.FromMinutes(1), cancellationToken);
    }
}
