namespace RoadRegistry.BackOffice.Handlers.Extracts;

using System.IO.Compression;
using Autofac;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Editor.Schema;
using Exceptions;
using FeatureCompare;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Polly;
using SqlStreamStore;
using TicketingService.Abstractions;

public class RoadNetworkExtractEventModule : EventHandlerModule
{
    private readonly ILifetimeScope _lifetimeScope;

    public RoadNetworkExtractEventModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
        IRoadNetworkExtractArchiveAssembler assembler,
        IStreamStore store,
        ApplicationMetadata applicationMetadata,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILogger<RoadNetworkExtractEventModule> logger)
    {
        _lifetimeScope = lifetimeScope.ThrowIfNull();

        ArgumentNullException.ThrowIfNull(downloadsBlobClient);
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(assembler);
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
                var changeRequestId = ChangeRequestId.FromUploadId(uploadId);
                var extractRequestId = ExtractRequestId.FromString(message.Body.RequestId);

                var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);

                try
                {
                    var roadRegistryContext = container.Resolve<IRoadRegistryContext>();
                    var extract = await roadRegistryContext.RoadNetworkExtracts.Get(extractRequestId, ct);

                    var featureCompareTranslatorFactory = container.Resolve<IZipArchiveFeatureCompareTranslatorFactory>();
                    var featureCompareTranslator = featureCompareTranslatorFactory.Create(extract.ZipArchiveWriterVersion);

                    await using var archiveBlobStream = await archiveBlob.OpenAsync(ct);
                    using var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false);

                    var translatedChanges = await featureCompareTranslator.TranslateAsync(archive, ct);
                    translatedChanges = translatedChanges.WithOperatorName(new OperatorName(message.ProvenanceData.Operator));

                    var changeRoadNetwork = await translatedChanges.ToChangeRoadNetworkCommand(
                        logger,
                        extractRequestId, changeRequestId, downloadId, message.Body.TicketId, ct);

                    var command = new Command(changeRoadNetwork)
                        .WithMessageId(message.MessageId)
                        .WithProvenanceData(message.ProvenanceData);
                    await queue.WriteAsync(command, ct);
                }
                catch (ZipArchiveValidationException ex)
                {
                    var rejectedChangeEvent = new RoadNetworkExtractChangesArchiveRejected
                    {
                        Description = message.Body.Description,
                        ExternalRequestId = message.Body.ExternalRequestId,
                        RequestId = changeRequestId,
                        DownloadId = downloadId,
                        UploadId = uploadId,
                        ArchiveId = archiveId,
                        Problems = ex.Problems.Select(problem => problem.Translate()).ToArray(),
                        TicketId = message.Body.TicketId
                    };

                    await roadNetworkEventWriter.WriteAsync(RoadNetworkExtracts.ToStreamName(extractRequestId), message.StreamVersion, new Event(
                        rejectedChangeEvent
                    ).WithMessageId(message.MessageId), ct);

                    if (message.Body.TicketId is not null)
                    {
                        var ticketing = container.Resolve<ITicketing>();
                        var errors = ex.Problems.Select(x => x.Translate().ToTicketError()).ToArray();
                        await ticketing.Error(message.Body.TicketId.Value, new TicketError(errors), ct);
                    }

                    await extractUploadFailedEmailClient.SendAsync(message.Body.Description, new ValidationException(JsonConvert.SerializeObject(rejectedChangeEvent, Formatting.Indented)), ct);
                }
                finally
                {
                    logger.LogInformation("Event handler finished for {EventName}", message.Body.GetType().Name);
                }
            });
    }

    private async Task RoadNetworkExtractRequestHandler<TMessage>(
        IRoadNetworkExtractCommandQueue queue,
        IRoadNetworkExtractArchiveAssembler assembler,
        RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
        Event<TMessage> message,
        CancellationToken ct)
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

        var overlappingDownloadIds = !message.Body.IsInformative
            ? await GetOverlappingDownloadIds(message.Body.DownloadId, message.Body.Contour, ct)
            : [];

        var policy = Policy
            .HandleResult<bool>(exists => !exists)
            .WaitAndRetryAsync([
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5)
            ]);
        var blobExists = await policy.ExecuteAsync(() => downloadsBlobClient.BlobExistsAsync(blobName, ct));
        if (blobExists)
        {
            await queue.Write(new Command(
                new AnnounceRoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = message.Body.RequestId,
                    DownloadId = message.Body.DownloadId,
                    ArchiveId = archiveId,
                    IsInformative = message.Body.IsInformative,
                    OverlapsWithDownloadIds = overlappingDownloadIds,
                    ZipArchiveWriterVersion = message.Body.ZipArchiveWriterVersion
                })
                .WithMessageId(message.MessageId), ct);
        }
        else
        {
            var request = new RoadNetworkExtractAssemblyRequest(
                new ExternalExtractRequestId(message.Body.ExternalRequestId),
                new DownloadId(message.Body.DownloadId),
                new ExtractDescription(extractDescription),
                GeometryTranslator.Translate(message.Body.Contour),
                message.Body.IsInformative,
                message.Body.ZipArchiveWriterVersion);

            try
            {
                using (var content = await assembler.AssembleArchive(request, ct))
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
                            IsInformative = message.Body.IsInformative,
                            OverlapsWithDownloadIds = overlappingDownloadIds,
                            ZipArchiveWriterVersion = message.Body.ZipArchiveWriterVersion
                        })
                    .WithMessageId(message.MessageId), ct);
            }
            catch (SqlException ex) when (ex.Number.Equals(-2))
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

    private async Task<List<Guid>> GetOverlappingDownloadIds(Guid downloadId, RoadNetworkExtractGeometry extractGeometry, CancellationToken cancellationToken)
    {
        await using var container = _lifetimeScope.BeginLifetimeScope();
        await using var editorContext = container.Resolve<EditorContext>();

        var geometry = (Geometry)GeometryTranslator.Translate(extractGeometry);

        return await editorContext.GetOverlappingExtractDownloadIds(geometry, [downloadId], cancellationToken);
    }
}
