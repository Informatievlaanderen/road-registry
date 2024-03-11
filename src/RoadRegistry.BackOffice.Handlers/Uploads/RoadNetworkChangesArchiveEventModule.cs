namespace RoadRegistry.BackOffice.Handlers.Uploads;

using Autofac;
using BackOffice.Extracts;
using BackOffice.FeatureCompare;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.FeatureCompare.Readers;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using TicketingService.Abstractions;

public class RoadNetworkChangesArchiveEventModule : EventHandlerModule
{
    public RoadNetworkChangesArchiveEventModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkUploadsBlobClient client,
        IStreamStore store,
        ApplicationMetadata applicationMetadata,
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILogger<RoadNetworkChangesArchiveEventModule> logger)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(client);
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
                        var translatedChanges = await featureCompareTranslator.TranslateAsync(archive, ct);
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
                                OrganizationId = translatedChanges.Organization,
                                TicketId = message.Body.TicketId
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
                        Problems = ex.Problems.Select(problem => problem.Translate()).ToArray(),
                        TicketId = message.Body.TicketId
                    };

                    await roadNetworkEventWriter.WriteAsync(RoadNetworkChangesArchives.GetStreamName(archiveId), message.StreamVersion, new Event(
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
}
