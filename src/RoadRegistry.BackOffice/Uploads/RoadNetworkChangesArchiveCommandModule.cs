namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using System;
using System.IO.Compression;
using SqlStreamStore.Streams;

public class RoadNetworkChangesArchiveCommandModule : CommandHandlerModule
{
    public RoadNetworkChangesArchiveCommandModule(
        RoadNetworkUploadsBlobClient blobClient,
        IStreamStore store,
        Func<EventSourcedEntityMap> entityMapFactory,
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
            .UseRoadRegistryContext(store, entityMapFactory, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, _, ct) =>
            {
                var archiveId = new ArchiveId(message.Body.ArchiveId);

                logger.LogInformation("Download started for S3 blob {BlobName}", archiveId);
                var archiveBlob = await blobClient.GetBlobAsync(new BlobName(archiveId), ct);
                logger.LogInformation("Download completed for S3 blob {BlobName}", archiveId);
                
                RoadNetworkChangesArchive upload;
                await using (var uploadBlobStream = await archiveBlob.OpenAsync(ct))
                {
                    upload = RoadNetworkChangesArchive.Upload(archiveId, uploadBlobStream, message.Body.FeatureCompareCompleted);
                }
                
                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                {
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        logger.LogInformation("Validation started for archive with validator {Validator}", validator.GetType().Name);
                        upload.ValidateArchiveUsing(archive, validator);
                        logger.LogInformation("Validation completed for archive with validator {Validator}", validator.GetType().Name);
                    }
                    context.RoadNetworkChangesArchives.Add(upload);
                }
                logger.LogInformation("Command handler finished for {Command}", nameof(UploadRoadNetworkChangesArchive));
            });
    }
}
