namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;
using Uploads;

public class RoadNetworkExtractCommandModule : CommandHandlerModule
{
    public RoadNetworkExtractCommandModule(
        RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        IZipArchiveAfterFeatureCompareValidator validator,
        IClock clock,
        ILogger<RoadNetworkExtractCommandModule> logger)
    {
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(clock);

        For<RequestRoadNetworkExtract>()
            .UseValidator(new RequestRoadNetworkExtractValidator())
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                var externalRequestId = new ExternalExtractRequestId(message.Body.ExternalRequestId);
                var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
                var downloadId = new DownloadId(message.Body.DownloadId);
                var description = new ExtractDescription(message.Body.Description);
                var contour = GeometryTranslator.Translate(message.Body.Contour);
                var extract = await context.RoadNetworkExtracts.Get(requestId, ct);
                if (extract == null)
                {
                    extract = RoadNetworkExtract.Request(externalRequestId, downloadId, description, contour);
                    context.RoadNetworkExtracts.Add(extract);
                }
                else
                {
                    extract.RequestAgain(downloadId, contour);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(RequestRoadNetworkExtract));
            });

        For<AnnounceRoadNetworkExtractDownloadBecameAvailable>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                var requestId = ExtractRequestId.FromString(message.Body.RequestId);
                var downloadId = new DownloadId(message.Body.DownloadId);
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var extract = await context.RoadNetworkExtracts.Get(requestId, ct);
                extract.AnnounceAvailable(downloadId, archiveId);

                logger.LogInformation("Command handler finished for {Command}", nameof(AnnounceRoadNetworkExtractDownloadBecameAvailable));
            });

        For<AnnounceRoadNetworkExtractDownloadTimeoutOccurred>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                var requestId = ExtractRequestId.FromString(message.Body.RequestId);
                var extract = await context.RoadNetworkExtracts.Get(requestId, ct);
                extract.AnnounceTimeoutOccurred();

                logger.LogInformation("Command handler finished for {Command}", nameof(AnnounceRoadNetworkExtractDownloadTimeoutOccurred));
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
                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                {
                    upload.ValidateArchiveUsing(archive, validator);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(UploadRoadNetworkExtractChangesArchive));
            });
    }
}
