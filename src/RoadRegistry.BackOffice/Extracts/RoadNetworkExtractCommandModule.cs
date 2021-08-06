namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Core;
    using Framework;
    using Messages;
    using NodaTime;
    using SqlStreamStore;
    using Uploads;

    public class RoadNetworkExtractCommandModule : CommandHandlerModule
    {
        public RoadNetworkExtractCommandModule(
            RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
            IStreamStore store,
            IRoadNetworkSnapshotReader snapshotReader,
            IZipArchiveValidator validator,
            IClock clock)
        {
            if (uploadsBlobClient == null) throw new ArgumentNullException(nameof(uploadsBlobClient));
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (snapshotReader == null) throw new ArgumentNullException(nameof(snapshotReader));
            if (clock == null) throw new ArgumentNullException(nameof(clock));

            For<RequestRoadNetworkExtract>()
                .UseValidator(new RequestRoadNetworkExtractValidator())
                .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
                .Handle(async (context, message, ct) =>
                {
                    var externalRequestId = new ExternalExtractRequestId(message.Body.ExternalRequestId);
                    var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
                    var downloadId = new DownloadId(message.Body.DownloadId);
                    var boundary = GeometryTranslator.Translate(message.Body.Contour);
                    var extract = await context.RoadNetworkExtracts.Get(requestId, ct);
                    if (extract == null)
                    {
                        extract = RoadNetworkExtract.Request(externalRequestId, downloadId, boundary);
                        context.RoadNetworkExtracts.Add(extract);
                    }
                    else
                    {
                        extract.RequestAgain(downloadId, boundary);
                    }
                });

            For<AnnounceRoadNetworkExtractDownloadBecameAvailable>()
                .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
                .Handle(async (context, message, ct) =>
                {
                    var requestId = ExtractRequestId.FromString(message.Body.RequestId);
                    var downloadId = new DownloadId(message.Body.DownloadId);
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var extract = await context.RoadNetworkExtracts.Get(requestId, ct);
                    extract.Announce(downloadId, archiveId);
                });

            For<UploadRoadNetworkExtractChangesArchive>()
                .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
                .Handle(async (context, message, ct) =>
                {
                    var requestId = ExtractRequestId.FromString(message.Body.RequestId);
                    var forDownloadId = new DownloadId(message.Body.DownloadId);
                    var uploadId = new UploadId(message.Body.UploadId);
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var extract = await context.RoadNetworkExtracts.Get(requestId, ct);

                    var upload = extract.Upload(forDownloadId, uploadId, archiveId);

                    var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);
                    using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        upload.ValidateArchiveUsing(archive, validator);
                    }
                });
        }
    }
}
