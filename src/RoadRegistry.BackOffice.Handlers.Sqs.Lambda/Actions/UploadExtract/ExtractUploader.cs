namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;

using System.IO.Compression;
using Abstractions.Exceptions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Exceptions;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.DutchTranslations;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.ValueObjects.ProblemCodes;
using TicketingService.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public interface IExtractUploader
{
    Task<TranslatedChanges> ProcessUploadAndDetectChanges(
        DownloadId downloadId,
        UploadId uploadId,
        TicketId ticketId,
        ZipArchiveMetadata zipArchiveMetadata,
        bool sendFailedEmail,
        CancellationToken cancellationToken);
}

public sealed class ExtractUploader : IExtractUploader
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("binary/octet-stream"),
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkUploadsBlobClient _uploadsBlobClient;
    private readonly TransactionZoneFeatureCompareFeatureReader _transactionZoneFeatureCompareFeatureReader;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IZipArchiveFeatureCompareTranslator _featureCompareTranslator;
    private readonly IExtractUploadFailedEmailClient _extractUploadFailedEmailClient;
    private readonly ITicketing _ticketing;

    public ExtractUploader(
        ExtractsDbContext extractsDbContext,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        TransactionZoneFeatureCompareFeatureReader transactionZoneFeatureCompareFeatureReader,
        IZipArchiveFeatureCompareTranslator featureCompareTranslator,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ITicketing ticketing)
    {
        _extractsDbContext = extractsDbContext;
        _uploadsBlobClient = uploadsBlobClient;
        _transactionZoneFeatureCompareFeatureReader = transactionZoneFeatureCompareFeatureReader;
        _featureCompareTranslator = featureCompareTranslator;
        _extractUploadFailedEmailClient = extractUploadFailedEmailClient;
        _ticketing = ticketing;
    }

    public async Task<TranslatedChanges> ProcessUploadAndDetectChanges(
        DownloadId downloadId,
        UploadId uploadId,
        TicketId ticketId,
        ZipArchiveMetadata zipArchiveMetadata,
        bool sendFailedEmail,
        CancellationToken cancellationToken)
    {
        var extractUpload = await EnsureExtractUploadExists(uploadId, downloadId, ticketId, cancellationToken);
        await _ticketing.Pending(ticketId, new TicketResult(new { Status = extractUpload.Status.ToString() }), cancellationToken);

        var blobName = new BlobName(uploadId);

        var extractDownload = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        if (extractDownload is null)
        {
            throw new ExtractDownloadNotFoundException(downloadId);
        }

        if (extractDownload.IsInformative)
        {
            throw new ExtractRequestMarkedInformativeException(downloadId);
        }

        if (extractDownload.Closed)
        {
            throw new ExtractRequestClosedException(downloadId);
        }

        if (extractDownload.DownloadedOn is null)
        {
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException();
        }

        var extractRequest = await _extractsDbContext.ExtractRequests.SingleOrDefaultAsync(x => x.ExtractRequestId == extractDownload.ExtractRequestId && x.CurrentDownloadId == downloadId.ToGuid(), cancellationToken);
        if (extractRequest is null)
        {
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException();
        }

        var archiveBlob = await _uploadsBlobClient.GetBlobAsync(blobName, cancellationToken);
        if (archiveBlob is null)
        {
            throw new BlobNotFoundException(blobName);
        }

        if (!ContentType.TryParse(archiveBlob.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
        {
            throw new UnsupportedMediaTypeException(archiveBlob.ContentType);
        }

        try
        {
            extractDownload.LatestUploadId = uploadId;
            await _extractsDbContext.SaveChangesAsync(cancellationToken);

            await using var archiveBlobStream = await archiveBlob.OpenAsync(cancellationToken);
            using var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, true);

            if (zipArchiveMetadata.Inwinning)
            {
                var (transactionZones, problems) = _transactionZoneFeatureCompareFeatureReader.Read(archive, FeatureType.Change, new ZipArchiveFeatureReaderContext(zipArchiveMetadata));
                problems.ThrowIfError();

                var transactionZone = transactionZones.Single();
                if (!transactionZone.Attributes.Geometry.Value.EqualsTopologically(extractDownload.Contour))
                {
                    var error = ExtractFileName.Transactiezones.AtShapeRecord(FeatureType.Change, transactionZone.RecordNumber)
                        .Error(ProblemCode.TransactionZone.HasChanged)
                        .Build();
                    throw new ZipArchiveValidationException(ZipArchiveProblems.Single(error));
                }
            }

            var translatedChanges = await _featureCompareTranslator.TranslateAsync(archive, zipArchiveMetadata.WithDownloadId(downloadId), cancellationToken);
            return translatedChanges;
        }
        catch (InvalidDataException)
        {
            throw new CorruptArchiveException();
        }
        catch (ZipArchiveValidationException ex)
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(uploadId, cancellationToken);
            if (sendFailedEmail)
            {
                await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            }
            throw ex.ToDutchValidationException(FileProblemTranslator.DomainV2);
        }
        catch
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(uploadId, cancellationToken);
            if (sendFailedEmail)
            {
                await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            }
            throw;
        }
    }

    private async Task<ExtractUpload> EnsureExtractUploadExists(UploadId uploadId, DownloadId downloadId, TicketId ticketId, CancellationToken cancellationToken)
    {
        var extractUpload = await _extractsDbContext.ExtractUploads.SingleOrDefaultAsync(x => x.UploadId == uploadId.ToGuid(), cancellationToken);
        if (extractUpload is null)
        {
            extractUpload = new ExtractUpload
            {
                UploadId = uploadId,
                DownloadId = downloadId,
                UploadedOn = DateTimeOffset.UtcNow,
                Status = ExtractUploadStatus.Processing,
                TicketId = ticketId
            };
            _extractsDbContext.ExtractUploads.Add(extractUpload);

            _extractsDbContext.ExtractUploadStatusHistory.Add(new ExtractUploadStatusHistory
            {
                UploadId = uploadId,
                Status = ExtractUploadStatus.Processing,
                Timestamp = extractUpload.UploadedOn
            });
        }
        else if(extractUpload.Status != ExtractUploadStatus.Processing)
        {
            extractUpload.Status = ExtractUploadStatus.Processing;
            extractUpload.TicketId = ticketId;

            _extractsDbContext.ExtractUploadStatusHistory.Add(new ExtractUploadStatusHistory
            {
                UploadId = uploadId,
                Status = extractUpload.Status,
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        return extractUpload;
    }

    private async Task HandleSendingFailedEmail(ExtractRequest extractRequest, DownloadId downloadId, CancellationToken cancellationToken)
    {
        if (extractRequest.ExternalRequestId is not null)
        {
            await _extractUploadFailedEmailClient.SendAsync(new(downloadId, extractRequest.Description), cancellationToken);
        }
    }
}
