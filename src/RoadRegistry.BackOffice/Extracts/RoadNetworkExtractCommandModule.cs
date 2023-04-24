namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.IO.Compression;
using Autofac;
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
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IZipArchiveAfterFeatureCompareValidator validator,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkExtractCommandModule>();

        For<RequestRoadNetworkExtract>()
            .UseValidator(new RequestRoadNetworkExtractValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(RequestRoadNetworkExtract));

                var externalRequestId = new ExternalExtractRequestId(message.Body.ExternalRequestId);
                var extractRequestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
                var extractDescription = new ExtractDescription(message.Body.Description);

                var downloadId = new DownloadId(message.Body.DownloadId);
                var contour = GeometryTranslator.Translate(message.Body.Contour);

                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);
                if (extract == null)
                {
                    extract = RoadNetworkExtract.Request(externalRequestId, downloadId, extractDescription, contour);
                    context.RoadNetworkExtracts.Add(extract);
                }
                else
                {
                    extract.RequestAgain(downloadId, contour);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(RequestRoadNetworkExtract));
            });

        For<AnnounceRoadNetworkExtractDownloadBecameAvailable>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(AnnounceRoadNetworkExtractDownloadBecameAvailable));

                var downloadId = new DownloadId(message.Body.DownloadId);
                var archiveId = new ArchiveId(message.Body.ArchiveId);

                var extractRequestId = ExtractRequestId.FromString(message.Body.RequestId);
                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);
                extract.AnnounceAvailable(downloadId, archiveId);

                logger.LogInformation("Command handler finished for {Command}", nameof(AnnounceRoadNetworkExtractDownloadBecameAvailable));
            });

        For<AnnounceRoadNetworkExtractDownloadTimeoutOccurred>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(AnnounceRoadNetworkExtractDownloadTimeoutOccurred));

                var extractRequestId = ExtractRequestId.FromString(message.Body.RequestId);
                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);
                extract.AnnounceTimeoutOccurred();

                logger.LogInformation("Command handler finished for {Command}", nameof(AnnounceRoadNetworkExtractDownloadTimeoutOccurred));
            });

        For<UploadRoadNetworkExtractChangesArchive>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(UploadRoadNetworkExtractChangesArchive));

                var downloadId = new DownloadId(message.Body.DownloadId);
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var uploadId = new UploadId(message.Body.UploadId);

                var extractRequestId = ExtractRequestId.FromString(message.Body.RequestId);
                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);

                var upload = extract.Upload(downloadId, uploadId, archiveId, message.Body.FeatureCompareCompleted);

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
