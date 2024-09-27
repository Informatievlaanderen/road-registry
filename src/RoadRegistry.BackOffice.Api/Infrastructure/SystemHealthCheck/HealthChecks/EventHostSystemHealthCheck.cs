namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck.HealthChecks;

using System;
using System.Collections.Generic;
using System.Reflection;
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
    private readonly SystemHealthCheckOptions _options;

    public EventHostSystemHealthCheck(
        ITicketing ticketing,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        RoadNetworkUploadsBlobClient roadNetworkUploadsBlobClient,
        SystemHealthCheckOptions options)
    {
        _ticketing = ticketing;
        _roadNetworkEventWriter = roadNetworkEventWriter;
        _roadNetworkUploadsBlobClient = roadNetworkUploadsBlobClient;
        _options = options;
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
                AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                BucketFileName = fileName
            }),
            cancellationToken);

        return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, _options.CheckTimeout, cancellationToken);
    }
}
