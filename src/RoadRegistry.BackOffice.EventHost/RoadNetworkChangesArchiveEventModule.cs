namespace RoadRegistry.BackOffice.EventHost;

using System;
using System.IO.Compression;
using System.Linq;
using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using FeatureCompare;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using TicketingService.Abstractions;
using Uploads;

public class RoadNetworkChangesArchiveEventModule : EventHandlerModule
{
    public RoadNetworkChangesArchiveEventModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        IStreamStore store,
        ApplicationMetadata applicationMetadata,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(applicationMetadata);
        ArgumentNullException.ThrowIfNull(roadNetworkEventWriter);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkChangesArchiveEventModule>();

        For<RoadNetworkChangesArchiveAccepted>()
            .UseRoadNetworkCommandQueue(store, applicationMetadata)
            .Handle(async (queue, message, ct) =>
            {
                await using var container = lifetimeScope.BeginLifetimeScope();

                logger.LogInformation("Event handler started for {EventName}", message.Body.GetType().Name);

                var downloadId = new DownloadId(message.Body.DownloadId!.Value);
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var extractRequestId = ExtractRequestId.FromString(message.Body.ExtractRequestId);
                var requestId = ChangeRequestId.FromArchiveId(archiveId);

                var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);

                var roadRegistryContext = container.Resolve<IRoadRegistryContext>();
                var extract = await roadRegistryContext.RoadNetworkExtracts.Get(extractRequestId, ct);

                try
                {
                    await using var archiveBlobStream = await archiveBlob.OpenAsync(ct);
                    using var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false);

                    var featureCompareTranslatorFactory = container.Resolve<IZipArchiveFeatureCompareTranslatorFactory>();
                    var featureCompareTranslator = featureCompareTranslatorFactory.Create(extract.ZipArchiveWriterVersion);

                    var translatedChanges = await featureCompareTranslator.TranslateAsync(archive, ct);
                    translatedChanges = translatedChanges.WithOperatorName(new OperatorName(message.ProvenanceData.Operator));

                    var changeRoadNetwork = await translatedChanges.ToChangeRoadNetworkCommand(
                        logger,
                        extractRequestId, requestId, downloadId, message.Body.TicketId, ct);

                    var command = new Command(changeRoadNetwork)
                        .WithMessageId(message.MessageId)
                        .WithProvenanceData(message.ProvenanceData);

                    await queue.WriteAsync(command, ct);
                }
                catch (ZipArchiveValidationException ex)
                {
                    var rejectedChangeEvent = new RoadNetworkChangesArchiveRejected
                    {
                        ArchiveId = archiveId,
                        Description = message.Body.Description,
                        Problems = ex.Problems.Select(problem => problem.Translate()).ToArray(),
                        TicketId = message.Body.TicketId
                    };

                    await roadNetworkEventWriter.WriteAsync(RoadNetworkChangesArchives.ToStreamName(archiveId), message.StreamVersion, new Event(
                        rejectedChangeEvent
                    ).WithMessageId(message.MessageId), ct);

                    if (message.Body.TicketId is not null)
                    {
                        var ticketing = container.Resolve<ITicketing>();
                        var errors = ex.Problems.Select(x => x.Translate().ToTicketError()).ToArray();
                        await ticketing.Error(message.Body.TicketId.Value, new TicketError(errors), ct);
                    }
                }
                finally
                {
                    logger.LogInformation("Event handler finished for {EventName}", message.Body.GetType().Name);
                }
            });
    }
}
