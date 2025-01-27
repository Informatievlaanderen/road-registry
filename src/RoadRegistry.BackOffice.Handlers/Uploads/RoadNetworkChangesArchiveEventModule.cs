namespace RoadRegistry.BackOffice.Handlers.Uploads;

using System.IO.Compression;
using Autofac;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Exceptions;
using FeatureCompare;
using FeatureCompare.Readers;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using TicketingService.Abstractions;

public class RoadNetworkChangesArchiveEventModule : EventHandlerModule
{
    public RoadNetworkChangesArchiveEventModule(
        ILifetimeScope lifetimeScope,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        IStreamStore store,
        ApplicationMetadata applicationMetadata,
        ITransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(lifetimeScope);
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(applicationMetadata);
        ArgumentNullException.ThrowIfNull(transactionZoneFeatureReader);
        ArgumentNullException.ThrowIfNull(roadNetworkEventWriter);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkChangesArchiveEventModule>();

        For<RoadNetworkChangesArchiveAccepted>()
            .UseRoadNetworkCommandQueue(store, applicationMetadata)
            .Handle(async (queue, message, ct) =>
            {
                await using var container = lifetimeScope.BeginLifetimeScope();

                logger.LogInformation("Event handler started for {EventName}", message.Body.GetType().Name);

                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var extractRequestId = ExtractRequestId.FromString(message.Body.ExtractRequestId);
                var requestId = ChangeRequestId.FromArchiveId(archiveId);

                var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);

                try
                {
                    var featureCompareTranslator = container.Resolve<IZipArchiveFeatureCompareTranslator>();

                    await using var archiveBlobStream = await archiveBlob.OpenAsync(ct);
                    using var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false);
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

                    var changeRoadNetwork = new ChangeRoadNetwork
                    {
                        ExtractRequestId = extractRequestId,
                        RequestId = requestId,
                        DownloadId = downloadId,
                        Changes = requestedChanges.ToArray(),
                        Reason = translatedChanges.Reason,
                        Operator = translatedChanges.Operator,
                        OrganizationId = translatedChanges.Organization,
                        TicketId = message.Body.TicketId
                    };

                    var validator = new ChangeRoadNetworkValidator();
                    var result = await validator.ValidateAsync(changeRoadNetwork, ct);

                    if (!result.IsValid)
                    {
                        var zipArchiveProblems = result.Errors
                            .Aggregate(
                                ZipArchiveProblems.None,
                                (current, error) => current.Add(new FileError(string.Empty, error.ErrorMessage)));
                        throw new ZipArchiveValidationException(zipArchiveProblems);
                    }

                    var command = new Command(changeRoadNetwork)
                        .WithMessageId(message.MessageId);

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
