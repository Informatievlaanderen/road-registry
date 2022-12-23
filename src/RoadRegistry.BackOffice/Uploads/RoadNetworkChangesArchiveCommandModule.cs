namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO.Compression;
using Amazon.Runtime.Internal.Util;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public class RoadNetworkChangesArchiveCommandModule : CommandHandlerModule
{
    public RoadNetworkChangesArchiveCommandModule(
        RoadNetworkUploadsBlobClient blobClient,
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        IZipArchiveAfterFeatureCompareValidator validator,
        IClock clock,
        ILogger<RoadNetworkChangesArchiveCommandModule> logger)
    {
        ArgumentNullException.ThrowIfNull(blobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logger);

        For<UploadRoadNetworkChangesArchive>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var upload = RoadNetworkChangesArchive.Upload(archiveId);

                logger.LogInformation("Download started for S3 blob {BlobName}", archiveId);
                var archiveBlob = await blobClient.GetBlobAsync(new BlobName(archiveId), ct);
                logger.LogInformation("Download completed for S3 blob {BlobName}", archiveId);

                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                {
                    logger.LogInformation("Validation started for archive with validator {Validator}", validator.GetType().Name);
                    upload.ValidateArchiveUsing(archive, validator);
                }

                logger.LogInformation("Validation started for archive with validator {Validator}", validator.GetType().Name);
                context.RoadNetworkChangesArchives.Add(upload);

                logger.LogInformation("Command handler finished for {Command}", nameof(UploadRoadNetworkChangesArchive));
            });
    }
}
