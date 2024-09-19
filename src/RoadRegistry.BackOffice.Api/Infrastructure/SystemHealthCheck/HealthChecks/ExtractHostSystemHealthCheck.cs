namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck.HealthChecks;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Framework;
using Messages;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqlStreamStore.Streams;
using TicketingService.Abstractions;

internal class ExtractHostSystemHealthCheck : ISystemHealthCheck
{
    private readonly ITicketing _ticketing;
    private readonly IRoadNetworkEventWriter _roadNetworkExtractWriter;
    private readonly RoadNetworkUploadsBlobClient _roadNetworkUploadsBlobClient;
    private readonly RoadNetworkExtractDownloadsBlobClient _roadNetworkExtractDownloadsBlobClient;

    public ExtractHostSystemHealthCheck(
        ITicketing ticketing,
        IRoadNetworkEventWriter roadNetworkExtractWriter,
        RoadNetworkUploadsBlobClient roadNetworkUploadsBlobClient,
        RoadNetworkExtractDownloadsBlobClient roadNetworkExtractDownloadsBlobClient)
    {
        _ticketing = ticketing;
        _roadNetworkExtractWriter = roadNetworkExtractWriter;
        _roadNetworkUploadsBlobClient = roadNetworkUploadsBlobClient;
        _roadNetworkExtractDownloadsBlobClient = roadNetworkExtractDownloadsBlobClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        var ticketId = await _ticketing.CreateTicket(new Dictionary<string, string>
        {
            { "Action", "HealthCheck"}
        }, cancellationToken);

        var fileName = "healthcheck-extracthost.bin";
        Task.WaitAll(
            _roadNetworkUploadsBlobClient.CreateDummyFile(fileName, cancellationToken),
            _roadNetworkExtractDownloadsBlobClient.CreateDummyFile(fileName, cancellationToken)
        );

        await _roadNetworkExtractWriter.WriteAsync(
            new StreamName("healthcheck"),
            ExpectedVersion.Any,
            new Event(new ExtractHostSystemHealthCheckRequested
            {
                TicketId = ticketId,
                AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                BucketFileName = fileName
            }),
            cancellationToken);

        return await _ticketing.WaitUntilCompleteOrTimeout(ticketId, TimeSpan.FromMinutes(1), cancellationToken);
    }
}
