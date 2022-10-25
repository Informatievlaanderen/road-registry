namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Framework;
using Messages;
using NodaTime;
using SqlStreamStore;

public class RoadNetworkChangesArchiveCommandModule : CommandHandlerModule
{
    public RoadNetworkChangesArchiveCommandModule(
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        IZipArchiveAfterFeatureCompareValidator validator,
        IClock clock)
    {
        if (uploadsBlobClient == null) throw new ArgumentNullException(nameof(uploadsBlobClient));
        if (store == null) throw new ArgumentNullException(nameof(store));
        if (snapshotReader == null) throw new ArgumentNullException(nameof(snapshotReader));
        if (validator == null) throw new ArgumentNullException(nameof(validator));
        if (clock == null) throw new ArgumentNullException(nameof(clock));

        For<UploadRoadNetworkChangesArchive>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var upload = RoadNetworkChangesArchive.Upload(archiveId);
                var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);
                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                {
                    upload.ValidateArchiveUsing(archive, validator);
                }

                context.RoadNetworkChangesArchives.Add(upload);
            });
    }
}