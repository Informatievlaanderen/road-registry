namespace RoadRegistry.BackOffice.Handlers.Uploads;

using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using BackOffice.FeatureCompare;
using BackOffice.FeatureCompare.Translators;

public class RoadNetworkChangesArchiveEventModule : EventHandlerModule
{
    public RoadNetworkChangesArchiveEventModule(
        RoadNetworkUploadsBlobClient client,
        IZipArchiveTranslator translator,
        IZipArchiveFeatureCompareTranslator featureCompareTranslator,
        IStreamStore store,
        ApplicationMetadata applicationMetadata,
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        ILogger<RoadNetworkChangesArchiveEventModule> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(translator);
        ArgumentNullException.ThrowIfNull(featureCompareTranslator);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(applicationMetadata);
        ArgumentNullException.ThrowIfNull(transactionZoneFeatureReader);

        For<RoadNetworkChangesArchiveAccepted>()
            .UseRoadNetworkCommandQueue(store, applicationMetadata)
            .Handle(async (queue, message, ct) =>
            {
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var requestId = ChangeRequestId.FromArchiveId(archiveId);
                var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);

                logger.LogInformation("Event handler started for {EventName}", nameof(RoadNetworkChangesArchiveAccepted));

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

                    var transactionZoneFeatures = transactionZoneFeatureReader.Read(archive.Entries, FeatureType.Change, "Transactiezones");
                    var downloadId = DownloadId.Parse(transactionZoneFeatures.Single().Attributes.DownloadId);

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

                    await queue.Write(command, ct);
                }

                logger.LogInformation("Event handler finished for {EventName}", nameof(RoadNetworkChangesArchiveAccepted));
            });
    }
}
