namespace RoadRegistry.BackOffice.Handlers.Uploads;

using BackOffice.Extracts;
using BackOffice.FeatureCompare;
using BackOffice.FeatureCompare.Translators;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using Autofac;
using FluentValidation;
using Newtonsoft.Json;

public class RoadNetworkChangesArchiveEventModule : EventHandlerModule
{
    public RoadNetworkChangesArchiveEventModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkUploadsBlobClient client,
        IZipArchiveTranslator translator,
        IStreamStore store,
        ApplicationMetadata applicationMetadata,
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILogger<RoadNetworkChangesArchiveEventModule> logger)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(translator);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(applicationMetadata);
        ArgumentNullException.ThrowIfNull(transactionZoneFeatureReader);
        ArgumentNullException.ThrowIfNull(roadNetworkEventWriter);
        ArgumentNullException.ThrowIfNull(logger);

        For<RoadNetworkChangesArchiveAccepted>()
            .UseRoadNetworkCommandQueue(store, applicationMetadata)
            .Handle(async (queue, message, ct) =>
            {
                await using var container = lifetimeScope.BeginLifetimeScope();

                logger.LogInformation("Event handler started for {EventName}", message.Body.GetType().Name);

                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var requestId = ChangeRequestId.FromArchiveId(archiveId);

                var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);

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

                        var readerContext = new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty);
                        var transactionZoneFeatures = transactionZoneFeatureReader.Read(archive, FeatureType.Change, ExtractFileName.Transactiezones, readerContext).Item1;
                        var downloadId = transactionZoneFeatures.Single().Attributes.DownloadId;

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
                    var rejectedChangeEvent = new RoadNetworkChangesArchiveRejected
                    {
                        ArchiveId = archiveId,
                        Description = message.Body.Description,
                        Problems = ex.Problems.Select(problem => problem.Translate()).ToArray()
                    };

                    await roadNetworkEventWriter.WriteAsync(RoadNetworkChangesArchives.GetStreamName(archiveId), message.StreamVersion, new Event(
                        rejectedChangeEvent
                    ).WithMessageId(message.MessageId), ct);

                    await extractUploadFailedEmailClient.SendAsync(message.Body.Description, new ValidationException(JsonConvert.SerializeObject(rejectedChangeEvent, Formatting.Indented)), ct);
                }
                finally
                {
                    logger.LogInformation("Event handler finished for {EventName}", message.Body.GetType().Name);
                }
            });
    }
}
