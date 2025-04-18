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
using System.Linq;
using Autofac;
using FeatureCompare;
using TicketingService.Abstractions;

public class RoadNetworkChangesArchiveCommandModule : CommandHandlerModule
{
    public RoadNetworkChangesArchiveCommandModule(
        RoadNetworkUploadsBlobClient blobClient,
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IZipArchiveBeforeFeatureCompareValidatorFactory beforeFeatureCompareValidatorFactory,
        ITransactionZoneZipArchiveReader transactionZoneFeatureReader,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(blobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(beforeFeatureCompareValidatorFactory);
        ArgumentNullException.ThrowIfNull(transactionZoneFeatureReader);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkChangesArchiveCommandModule>();

        For<UploadRoadNetworkChangesArchive>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, _, ct) =>
            {
                await using var container = lifetimeScope.BeginLifetimeScope();

                logger.LogInformation("Command handler started for {Command}", nameof(UploadRoadNetworkChangesArchive));

                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var downloadId = new DownloadId(message.Body.DownloadId);
                var extractRequestId = ExtractRequestId.FromString(message.Body.ExtractRequestId);

                logger.LogInformation("Download started for S3 blob {BlobName}", archiveId);
                var archiveBlob = await blobClient.GetBlobAsync(new BlobName(archiveId), ct);
                logger.LogInformation("Download completed for S3 blob {BlobName}", archiveId);

                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                {
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        logger.LogInformation("Validation started for archive");
                        var beforeFeatureCompareValidator = beforeFeatureCompareValidatorFactory.Create(message.Body.ZipArchiveWriterVersion);
                        var problems = await beforeFeatureCompareValidator.ValidateAsync(archive, ZipArchiveMetadata.Empty, ct);

                        var extractDescription = transactionZoneFeatureReader.Read(archive).Description;

                        var upload = RoadNetworkChangesArchive.Upload(archiveId, extractDescription, message.Body.TicketId);
                        upload.AcceptOrReject(problems, extractRequestId, downloadId, message.Body.TicketId);

                        if (problems.HasError() && message.Body.TicketId is not null)
                        {
                            var ticketing = container.Resolve<ITicketing>();
                            var errors = problems.Select(x => x.Translate().ToTicketError()).ToArray();
                            await ticketing.Error(message.Body.TicketId.Value, new TicketError(errors), ct);
                        }

                        logger.LogInformation("Validation completed for archive");

                        context.RoadNetworkChangesArchives.Add(upload);
                    }
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(UploadRoadNetworkChangesArchive));
            });
    }
}
