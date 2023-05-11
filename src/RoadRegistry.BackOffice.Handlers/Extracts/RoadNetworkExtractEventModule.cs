namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Extracts;
using BackOffice.FeatureCompare;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Microsoft.Data.SqlClient;
using Polly;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore;

public class RoadNetworkExtractEventModule : EventHandlerModule
{
    public RoadNetworkExtractEventModule(
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
        IRoadNetworkExtractArchiveAssembler assembler,
        IZipArchiveTranslator translator,
        IZipArchiveFeatureCompareTranslator featureCompareTranslator,
        IStreamStore store,
        ApplicationMetadata applicationMetadata)
    {
        ArgumentNullException.ThrowIfNull(downloadsBlobClient);
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(assembler);
        ArgumentNullException.ThrowIfNull(translator);
        ArgumentNullException.ThrowIfNull(featureCompareTranslator);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(applicationMetadata);
        
        For<RoadNetworkExtractGotRequested>()
            .UseRoadNetworkExtractCommandQueue(store)
            .Handle(async (queue, message, ct) => await RoadNetworkExtractRequestHandler(queue, assembler, downloadsBlobClient, message, ct));

        For<RoadNetworkExtractGotRequestedV2>()
            .UseRoadNetworkExtractCommandQueue(store)
            .Handle(async (queue, message, ct) => await RoadNetworkExtractRequestHandler(queue, assembler, downloadsBlobClient, message, ct));

        For<RoadNetworkExtractChangesArchiveAccepted>()
            .UseRoadNetworkCommandQueue(store, applicationMetadata)
            .Handle(async (queue, message, ct) =>
            {
                var uploadId = new UploadId(message.Body.UploadId);
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var requestId = ChangeRequestId.FromUploadId(uploadId);
                var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);
                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                {
                    var requestedChanges = new List<RequestedChange>();
                    var translatedChanges = message.Body.UseZipArchiveFeatureCompareTranslator
                        ? await featureCompareTranslator.Translate(archive, ct)
                        : translator.Translate(archive);
                    foreach (var change in translatedChanges)
                    {
                        var requestedChange = new RequestedChange();
                        change.TranslateTo(requestedChange);
                        requestedChanges.Add(requestedChange);
                    }

                    var command = new Command(new ChangeRoadNetwork
                        {
                            RequestId = requestId,
                            Changes = requestedChanges.ToArray(),
                            Reason = translatedChanges.Reason,
                            Operator = translatedChanges.Operator,
                            OrganizationId = translatedChanges.Organization
                        })
                        .WithMessageId(message.MessageId);

                    await queue.Write(command, ct);
                }
            });
    }

    private async Task RoadNetworkExtractRequestHandler<TMessage>(IRoadNetworkExtractCommandQueue queue, IRoadNetworkExtractArchiveAssembler assembler, RoadNetworkExtractDownloadsBlobClient downloadsBlobClient, Event<TMessage> message, CancellationToken ct)
        where TMessage : IRoadNetworkExtractGotRequestedMessage
    {
        var archiveId = new ArchiveId(message.Body.DownloadId.ToString("N"));
        var blobName = new BlobName(archiveId);
        var extractDescription = message switch
        {
            Event<RoadNetworkExtractGotRequested> v1 => v1.Body.Description ?? string.Empty,
            Event<RoadNetworkExtractGotRequestedV2> v2 => v2.Body.Description,
            _ => throw new NotSupportedException($"Message type '{message.GetType().Name}' does not have support to extract the description")
        };

        var policy = Policy
            .HandleResult<bool>(exists => !exists)
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5)
            });
        var blobExists = await policy.ExecuteAsync(() => downloadsBlobClient.BlobExistsAsync(blobName, ct));

        if (blobExists)
        {
            await queue.Write(new Command(
                new AnnounceRoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = message.Body.RequestId,
                    DownloadId = message.Body.DownloadId,
                    ArchiveId = archiveId
                })
                .WithMessageId(message.MessageId), ct);
        }
        else
        {
            var request = new RoadNetworkExtractAssemblyRequest(
                new ExternalExtractRequestId(message.Body.ExternalRequestId),
                new DownloadId(message.Body.DownloadId),
                new ExtractDescription(extractDescription),
                GeometryTranslator.Translate(message.Body.Contour));

            try
            {
                using (var content = await assembler.AssembleArchive(request, ct)) //(content, revision)
                {
                    content.Position = 0L;

                    await downloadsBlobClient.CreateBlobAsync(
                        new BlobName(archiveId),
                        Metadata.None,
                        ContentType.Parse("application/x-zip-compressed"),
                        content,
                        ct);
                }

                await queue.Write(new Command(
                        new AnnounceRoadNetworkExtractDownloadBecameAvailable
                        {
                            RequestId = message.Body.RequestId,
                            DownloadId = message.Body.DownloadId,
                            //Revision = revision,
                            ArchiveId = archiveId
                        })
                    .WithMessageId(message.MessageId), ct);
            }
            catch (SqlException ex) when (ex.Number.Equals(-2))
            {
                await queue.Write(new Command(
                        new AnnounceRoadNetworkExtractDownloadTimeoutOccurred
                        {
                            RequestId = message.Body.RequestId
                        })
                    .WithMessageId(message.MessageId), ct);
            }
        }
    }
}
