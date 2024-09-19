namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthCheck.HealthChecks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Framework;
using Messages;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqlStreamStore.Streams;
using TicketingService.Abstractions;

internal class ExtractHostHealthCheck : ISystemHealthCheck
{
    private readonly ITicketing _ticketing;
    private readonly IRoadNetworkEventWriter _roadNetworkExtractWriter;
    private readonly RoadNetworkSnapshotsBlobClient _roadNetworkSnapshotsBlobClient;
    private readonly SqsMessagesBlobClient _sqsMessagesBlobClient;
    private readonly RoadNetworkUploadsBlobClient _roadNetworkUploadsBlobClient;
    private readonly RoadNetworkExtractDownloadsBlobClient _roadNetworkExtractDownloadsBlobClient;

    public ExtractHostHealthCheck(
        ITicketing ticketing,
        IRoadNetworkEventWriter roadNetworkExtractWriter,
        RoadNetworkSnapshotsBlobClient roadNetworkSnapshotsBlobClient,
        SqsMessagesBlobClient sqsMessagesBlobClient,
        RoadNetworkUploadsBlobClient roadNetworkUploadsBlobClient,
        RoadNetworkExtractDownloadsBlobClient roadNetworkExtractDownloadsBlobClient)
    {
        _ticketing = ticketing;
        _roadNetworkExtractWriter = roadNetworkExtractWriter;
        _roadNetworkSnapshotsBlobClient = roadNetworkSnapshotsBlobClient;
        _sqsMessagesBlobClient = sqsMessagesBlobClient;
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

        /*.AddS3(x => x
                    .CheckPermission(WellKnownBuckets.SnapshotsBucket, Permission.Read)
                    .CheckPermission(WellKnownBuckets.SqsMessagesBucket, Permission.Read)
                    .CheckPermission(WellKnownBuckets.UploadsBucket, Permission.Read)
                    .CheckPermission(WellKnownBuckets.ExtractDownloadsBucket, Permission.Read, Permission.Delete)
                )*/

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

    private async Task CreateDummyFile(IBlobClient blobClient, string fileName, CancellationToken cancellationToken)
    {


        var metadata = Metadata.None.Add(
            new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), fileName)
        );

        var blobName = new BlobName(fileName);

        if (await blobClient.BlobExistsAsync(blobName, cancellationToken))
        {
            await blobClient.DeleteBlobAsync(blobName, cancellationToken);
        }

        await using var emptyFileStream = new MemoryStream();

        await blobClient.CreateBlobAsync(
            new BlobName(fileName),
            metadata,
            ContentType.Parse("application/octet-stream"),
            emptyFileStream,
            cancellationToken
        );
    }
}
