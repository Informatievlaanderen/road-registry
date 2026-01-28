namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public sealed class ExtractUploader
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("binary/octet-stream"),
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkUploadsBlobClient _uploadsBlobClient;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IZipArchiveFeatureCompareTranslator _featureCompareTranslator;
    private readonly IExtractUploadFailedEmailClient _extractUploadFailedEmailClient;

    public ExtractUploader(
        ExtractsDbContext extractsDbContext,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        IZipArchiveFeatureCompareTranslator featureCompareTranslator,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient)
    {
        _extractsDbContext = extractsDbContext;
        _uploadsBlobClient = uploadsBlobClient;
        _featureCompareTranslator = featureCompareTranslator;
        _extractUploadFailedEmailClient = extractUploadFailedEmailClient;
    }

    public async Task<TranslatedChanges> ProcessUploadAndDetectChanges(DownloadId downloadId, UploadId uploadId, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
    {
        var blobName = new BlobName(uploadId);

        var extractDownload = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        if (extractDownload is null)
        {
            throw new ExtractDownloadNotFoundException(downloadId);
        }

        extractDownload.UploadId = uploadId;
        extractDownload.UploadedOn = DateTimeOffset.UtcNow;

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

        extractDownload.UploadStatus = ExtractUploadStatus.Processing;
        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await using var archiveBlobStream = await archiveBlob.OpenAsync(cancellationToken);
            using var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, true);

            var translatedChanges = await _featureCompareTranslator.TranslateAsync(archive, zipArchiveMetadata.WithDownloadId(downloadId), cancellationToken);
            return translatedChanges;
        }
        catch (InvalidDataException)
        {
            throw new CorruptArchiveException();
        }
        catch (ZipArchiveValidationException ex)
        {
            await RejectExtractDownload(downloadId, cancellationToken);
            await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            throw ex.ToDutchValidationException();
        }
        catch
        {
            await RejectExtractDownload(downloadId, cancellationToken);
            await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            throw;
        }
    }

    private async Task RejectExtractDownload(DownloadId downloadId, CancellationToken cancellationToken)
    {
        var extractDownload = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        if (extractDownload is not null)
        {
            extractDownload.UploadStatus = ExtractUploadStatus.Rejected;
            await _extractsDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task HandleSendingFailedEmail(ExtractRequest extractRequest, DownloadId downloadId, CancellationToken cancellationToken)
    {
        if (extractRequest.ExternalRequestId is not null)
        {
            await _extractUploadFailedEmailClient.SendAsync(new(downloadId, extractRequest.Description), cancellationToken);
        }
    }
}
