namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;

using System.IO.Compression;
using Abstractions.Exceptions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public interface IExtractUploader
{
    Task<TranslatedChanges> ProcessUploadAndDetectChanges(DownloadId downloadId, UploadId uploadId, TicketId ticketId, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken);
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

    public async Task<TranslatedChanges> ProcessUploadAndDetectChanges(DownloadId downloadId, UploadId uploadId, TicketId ticketId, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
    {
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
        }
        else
        {
            extractUpload.Status = ExtractUploadStatus.Processing;
            extractUpload.TicketId = ticketId;
        }

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
            await RejectExtractUpload(uploadId, cancellationToken);
            await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            throw ex.ToDutchValidationException();
        }
        catch
        {
            await RejectExtractUpload(uploadId, cancellationToken);
            await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            throw;
        }
    }

    private async Task RejectExtractUpload(UploadId uploadId, CancellationToken cancellationToken)
    {
        var extractUpload = await _extractsDbContext.ExtractUploads.SingleAsync(x => x.UploadId == uploadId.ToGuid(), cancellationToken);
        extractUpload.Status = ExtractUploadStatus.AutomaticValidationFailed;

        await _extractsDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleSendingFailedEmail(ExtractRequest extractRequest, DownloadId downloadId, CancellationToken cancellationToken)
    {
        if (extractRequest.ExternalRequestId is not null)
        {
            await _extractUploadFailedEmailClient.SendAsync(new(downloadId, extractRequest.Description), cancellationToken);
        }
    }
}
