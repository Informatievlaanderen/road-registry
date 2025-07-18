namespace RoadRegistry.BackOffice.Extracts;

using Autofac;
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
using FeatureCompare;
using TicketingService.Abstractions;

public class RoadNetworkExtractCommandModule : CommandHandlerModule
{
    public RoadNetworkExtractCommandModule(
        RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
        IStreamStore store,
        ILifetimeScope lifetimeScope,
        IRoadNetworkSnapshotReader snapshotReader,
        IZipArchiveBeforeFeatureCompareValidatorFactory beforeFeatureCompareValidatorFactory,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        IClock clock,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(uploadsBlobClient);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(beforeFeatureCompareValidatorFactory);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var logger = loggerFactory.CreateLogger<RoadNetworkExtractCommandModule>();
        var eventEnricher = EnrichEvent.WithTime(clock);

        For<RequestRoadNetworkExtract>()
            .UseValidator(new RequestRoadNetworkExtractValidator())
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, eventEnricher)
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(RequestRoadNetworkExtract));

                var externalRequestId = new ExternalExtractRequestId(message.Body.ExternalRequestId);
                var extractRequestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
                var extractDescription = new ExtractDescription(message.Body.Description);

                var downloadId = new DownloadId(message.Body.DownloadId);
                var contour = GeometryTranslator.Translate(message.Body.Contour);
                var isInformative = message.Body.IsInformative;

                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);
                if (extract == null)
                {
                    extract = RoadNetworkExtract.Request(eventEnricher, externalRequestId, downloadId, extractDescription, contour, isInformative, message.Body.ZipArchiveWriterVersion);
                    context.RoadNetworkExtracts.Add(extract);
                }
                else
                {
                    extract.RequestAgain(downloadId, contour, isInformative, message.Body.ZipArchiveWriterVersion);
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(RequestRoadNetworkExtract));
            });

        For<DownloadRoadNetworkExtract>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, eventEnricher)
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(DownloadRoadNetworkExtract));

                var extractRequestId = ExtractRequestId.FromExternalRequestId(message.Body.ExternalRequestId);
                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);

                extract.Download(message.Body.DownloadId);

                logger.LogInformation("Command handler finished for {Command}", nameof(DownloadRoadNetworkExtract));
            });

        For<CloseRoadNetworkExtract>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, eventEnricher)
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(CloseRoadNetworkExtract));

                var extractRequestId = ExtractRequestId.FromExternalRequestId(message.Body.ExternalRequestId);
                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);

                extract.Close(message.Body.Reason, message.Body.DownloadId);

                logger.LogInformation("Command handler finished for {Command}", nameof(CloseRoadNetworkExtract));
            });

        For<AnnounceRoadNetworkExtractDownloadBecameAvailable>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, eventEnricher)
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(AnnounceRoadNetworkExtractDownloadBecameAvailable));

                var downloadId = new DownloadId(message.Body.DownloadId);
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var overlapsWithDownloadIds = message.Body.OverlapsWithDownloadIds?.Select(x => new DownloadId(x)).ToList() ?? [];

                var extractRequestId = ExtractRequestId.FromString(message.Body.RequestId);
                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);

                extract.AnnounceAvailable(downloadId, archiveId, overlapsWithDownloadIds, message.Body.ZipArchiveWriterVersion);

                logger.LogInformation("Command handler finished for {Command}", nameof(AnnounceRoadNetworkExtractDownloadBecameAvailable));
            });

        For<AnnounceRoadNetworkExtractDownloadTimeoutOccurred>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, eventEnricher)
            .Handle(async (context, message, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(AnnounceRoadNetworkExtractDownloadTimeoutOccurred));

                var extractRequestId = ExtractRequestId.FromString(message.Body.RequestId);
                var downloadId = DownloadId.FromValue(message.Body.DownloadId);

                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);
                extract.AnnounceTimeoutOccurred(downloadId);

                logger.LogInformation("Command handler finished for {Command}", nameof(AnnounceRoadNetworkExtractDownloadTimeoutOccurred));
            });

        For<UploadRoadNetworkExtractChangesArchive>()
            .UseRoadRegistryContext(store, lifetimeScope, snapshotReader, loggerFactory, eventEnricher)
            .Handle(async (context, command, _, ct) =>
            {
                logger.LogInformation("Command handler started for {Command}", nameof(UploadRoadNetworkExtractChangesArchive));

                var downloadId = new DownloadId(command.Body.DownloadId);
                var archiveId = new ArchiveId(command.Body.ArchiveId);
                var uploadId = new UploadId(command.Body.UploadId);
                var ticketId = TicketId.FromValue(command.Body.TicketId);

                var extractRequestId = ExtractRequestId.FromString(command.Body.RequestId);
                var extract = await context.RoadNetworkExtracts.Get(extractRequestId, ct);

                using (var container = lifetimeScope.BeginLifetimeScope())
                {
                    try
                    {
                        var upload = extract.Upload(downloadId, uploadId, archiveId, ticketId);

                        var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);
                        await using var archiveBlobStream = await archiveBlob.OpenAsync(ct);
                        using var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false);

                        var ticketing = container.Resolve<ITicketing>();
                        var beforeFeatureCompareValidator = beforeFeatureCompareValidatorFactory.Create(extract.ZipArchiveWriterVersion);

                        await upload.ValidateArchiveUsing(archive, ticketId, beforeFeatureCompareValidator, extractUploadFailedEmailClient, ticketing, ct);
                    }
                    catch (Exception ex)
                    {
                        if (extractUploadFailedEmailClient is not null)
                        {
                            await extractUploadFailedEmailClient.SendAsync(new (downloadId, extract.Description), ct);
                        }
                        throw;
                    }
                }

                logger.LogInformation("Command handler finished for {Command}", nameof(UploadRoadNetworkExtractChangesArchive));
            });
    }
}
