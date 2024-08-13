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
using Extracts;
using FeatureCompare;
using FeatureCompare.Readers;
using TicketingService.Abstractions;

public class RoadNetworkChangesArchiveCommandModule : CommandHandlerModule
{
    private readonly ITransactionZoneFeatureCompareFeatureReader _transactionZoneFeatureReader;

    public RoadNetworkChangesArchiveCommandModule(
        RoadNetworkUploadsBlobClient blobClient,
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IZipArchiveBeforeFeatureCompareValidator beforeFeatureCompareValidator,
        ITransactionZoneFeatureCompareFeatureReader transactionZoneFeatureReader,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(blobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(beforeFeatureCompareValidator);
        _transactionZoneFeatureReader = transactionZoneFeatureReader.ThrowIfNull();
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

                logger.LogInformation("Download started for S3 blob {BlobName}", archiveId);
                var archiveBlob = await blobClient.GetBlobAsync(new BlobName(archiveId), ct);
                logger.LogInformation("Download completed for S3 blob {BlobName}", archiveId);

                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                {
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        logger.LogInformation("Validation started for archive");
                        var problems = await beforeFeatureCompareValidator.ValidateAsync(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty), ct);

                        var extractDescription = ReadExtractDescriptionSafely(archive);

                        var upload = RoadNetworkChangesArchive.Upload(archiveId, extractDescription);
                        upload.AcceptOrReject(problems, message.Body.TicketId);

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

    private ExtractDescription ReadExtractDescriptionSafely(ZipArchive archive)
    {
        try
        {
            return _transactionZoneFeatureReader
                .Read(
                    archive,
                    FeatureType.Change,
                    ExtractFileName.Transactiezones,
                    new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)
                )
                .Item1
                .Single()
                .Attributes
                .Description;
        }
        catch
        {
            return new ExtractDescription();
        }
    }
}
