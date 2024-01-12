namespace RoadRegistry.BackOffice.Handlers.Extracts;

using BackOffice.Extracts;
using BackOffice.FeatureCompare;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Exceptions;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

public class RoadNetworkExtractEventModule : EventHandlerModule
{
    public RoadNetworkExtractEventModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
        IRoadNetworkExtractArchiveAssembler assembler,
        IZipArchiveTranslator translator,
        IStreamStore store,
        ApplicationMetadata applicationMetadata,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILogger<RoadNetworkExtractEventModule> logger)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(downloadsBlobClient);
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(assembler);
        ArgumentNullException.ThrowIfNull(translator);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(applicationMetadata);
        ArgumentNullException.ThrowIfNull(roadNetworkEventWriter);
        ArgumentNullException.ThrowIfNull(logger);

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
                await using var container = lifetimeScope.BeginLifetimeScope();

                logger.LogInformation("Event handler started for {EventName}", message.Body.GetType().Name);

                var uploadId = new UploadId(message.Body.UploadId);
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var downloadId = new DownloadId(message.Body.DownloadId);
                var requestId = ChangeRequestId.FromUploadId(uploadId);
                var extractRequestId = ExtractRequestId.FromString(message.Body.RequestId);

                var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);

                try
                {
                    var featureCompareTranslator = container.Resolve<IZipArchiveFeatureCompareTranslator>();

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
                                DownloadId = downloadId,
                                Changes = requestedChanges.ToArray(),
                                Reason = translatedChanges.Reason,
                                Operator = translatedChanges.Operator,
                                OrganizationId = translatedChanges.Organization
                            })
                            .WithMessageId(message.MessageId);

                        await queue.WriteAsync(command, ct);
                    }
                }
                catch (ZipArchiveValidationException ex)
                {
                    var rejectedChangeEvent = new RoadNetworkExtractChangesArchiveRejected
                    {
                        Description = message.Body.Description,
                        ExternalRequestId = message.Body.ExternalRequestId,
                        RequestId = requestId,
                        DownloadId = downloadId,
                        UploadId = uploadId,
                        ArchiveId = archiveId,
                        Problems = ex.Problems.Select(problem => problem.Translate()).ToArray()
                    };

                    await roadNetworkEventWriter.WriteAsync(RoadNetworkExtracts.ToStreamName(extractRequestId), message, message.StreamVersion, new object[]
                    {
                        rejectedChangeEvent
                    }, ct);

                    await extractUploadFailedEmailClient.SendAsync(message.Body.Description, new ValidationException(JsonConvert.SerializeObject(rejectedChangeEvent, Formatting.Indented)), ct);
                }
                finally
                {
                    logger.LogInformation("Event handler finished for {EventName}", message.Body.GetType().Name);
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
                    ArchiveId = archiveId,
                    IsInformative = message.Body.IsInformative
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

            var maxAttempt = 3;

            for (var attempt = 1; attempt <= maxAttempt; attempt++)
            {
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
                                ArchiveId = archiveId,
                                IsInformative = message.Body.IsInformative
                            })
                        .WithMessageId(message.MessageId), ct);
                    break;
                }
                catch (SqlException ex) when (ex.Number.Equals(-2))
                {
                    if (attempt == maxAttempt)
                    {
                        await queue.Write(new Command(
                                new AnnounceRoadNetworkExtractDownloadTimeoutOccurred
                                {
                                    RequestId = message.Body.RequestId,
                                    DownloadId = message.Body.DownloadId
                                })
                            .WithMessageId(message.MessageId), ct);
                    }
                }
            }
        }
    }
}
