namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using System.IO.Compression;
using Abstractions.Exceptions;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Core.ProblemCodes;
using Exceptions;
using FeatureCompare;
using Framework;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Requests.Extracts;
using RoadRegistry.Extracts.Schema;
using SqlStreamStore;
using TicketingService.Abstractions;
using ZipArchiveWriters.Cleaning;

public sealed class UploadExtractSqsLambdaRequestHandler : SqsLambdaHandler<UploadExtractSqsLambdaRequest>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("binary/octet-stream"),
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkUploadsBlobClient _uploadsBlobClient;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IZipArchiveBeforeFeatureCompareValidatorFactory _beforeFeatureCompareValidatorFactory;
    private readonly IStreamStore _store;
    private readonly IBeforeFeatureCompareZipArchiveCleanerFactory _beforeFeatureCompareZipArchiveCleanerFactory;
    private readonly IZipArchiveFeatureCompareTranslatorFactory _featureCompareTranslatorFactory;
    private readonly IExtractUploadFailedEmailClient _extractUploadFailedEmailClient;

    public UploadExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        IZipArchiveBeforeFeatureCompareValidatorFactory beforeFeatureCompareValidatorFactory,
        IStreamStore store,
        IBeforeFeatureCompareZipArchiveCleanerFactory beforeFeatureCompareZipArchiveCleanerFactory,
        IZipArchiveFeatureCompareTranslatorFactory featureCompareTranslatorFactory,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<UploadExtractSqsLambdaRequestHandler>())
    {
        _extractsDbContext = extractsDbContext;
        _uploadsBlobClient = uploadsBlobClient;
        _beforeFeatureCompareValidatorFactory = beforeFeatureCompareValidatorFactory;
        _store = store;
        _beforeFeatureCompareZipArchiveCleanerFactory = beforeFeatureCompareZipArchiveCleanerFactory;
        _featureCompareTranslatorFactory = featureCompareTranslatorFactory;
        _extractUploadFailedEmailClient = extractUploadFailedEmailClient;
    }

    protected override async Task<object> InnerHandle(UploadExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(request.Request.TicketId); // NOT the one from the SqsRequest
        var downloadId = new DownloadId(request.Request.DownloadId);

        try
        {
            var innerHandleResult = await InnerHandleForCustomTicketId(request, ticketId, cancellationToken);
            // Do not complete ticket here, it's done by ChangeRoadNetwork handler

            return innerHandleResult;
        }
        catch (Exception exception)
        {
            var ticketError = TryConvertToTicketError(exception, request);
            await Ticketing.Error(ticketId, ticketError, cancellationToken);

            var extractDownload = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
            if (extractDownload is not null)
            {
                extractDownload.UploadStatus = ExtractUploadStatus.Rejected;
                await _extractsDbContext.SaveChangesAsync(cancellationToken);
            }

            throw;
        }
    }

    private async Task<object> InnerHandleForCustomTicketId(UploadExtractSqsLambdaRequest request, TicketId ticketId, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(request.Request.DownloadId);
        var uploadId = new UploadId(request.Request.UploadId);
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

        var extractRequestId = ExtractRequestId.FromString(extractDownload.ExtractRequestId);

        var zipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.V2;

        try
        {
            await using var archiveBlobStream = await archiveBlob.OpenAsync(cancellationToken);
            await using var archiveStream = await archiveBlobStream.CopyToNewMemoryStreamAsync(cancellationToken);
            await CleanArchive(archiveStream, zipArchiveWriterVersion, cancellationToken);

            archiveStream.Position = 0;
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, true);

            var beforeFeatureCompareValidator = _beforeFeatureCompareValidatorFactory.Create(zipArchiveWriterVersion);
            var problems = await beforeFeatureCompareValidator.ValidateAsync(archive, ZipArchiveMetadata.Empty.WithDownloadId(downloadId), cancellationToken);
            problems.ThrowIfError();

            var featureCompareTranslator = _featureCompareTranslatorFactory.Create(zipArchiveWriterVersion);
            var translatedChanges = await featureCompareTranslator.TranslateAsync(archive, cancellationToken);
            translatedChanges = translatedChanges.WithOperatorName(new OperatorName(request.Provenance.Operator));

            var requestId = ChangeRequestId.FromUploadId(uploadId);
            var changeRoadNetwork = await translatedChanges.ToChangeRoadNetworkCommand(
                Logger,
                extractRequestId, requestId, downloadId, ticketId, cancellationToken);

            var command = new Command(changeRoadNetwork)
                .WithMessageId(request.Request.TicketId)
                .WithProvenanceData(request.Request.ProvenanceData);
            var roadNetworkCommandQueueForCommandHost = new RoadNetworkCommandQueue(_store, new ApplicationMetadata(RoadRegistryApplication.BackOffice));
            await roadNetworkCommandQueueForCommandHost.WriteAsync(command, cancellationToken);
        }
        catch (InvalidDataException)
        {
            throw new CorruptArchiveException();
        }
        catch (ZipArchiveValidationException ex)
        {
            await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            throw ex.ToDutchValidationException();
        }
        catch
        {
            await HandleSendingFailedEmail(extractRequest, downloadId, cancellationToken);
            throw;
        }

        return new object();
    }

    private async Task HandleSendingFailedEmail(ExtractRequest extractRequest, DownloadId downloadId, CancellationToken cancellationToken)
    {
        if (extractRequest.ExternalRequestId is not null)
        {
            await _extractUploadFailedEmailClient.SendAsync(new(downloadId, extractRequest.Description), cancellationToken);
        }
    }

    private async Task CleanArchive(Stream archiveStream, string zipArchiveWriterVersion, CancellationToken cancellationToken)
    {
        try
        {
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true);

            try
            {
                var cleaner = _beforeFeatureCompareZipArchiveCleanerFactory.Create(zipArchiveWriterVersion);
                await cleaner.CleanAsync(archive, cancellationToken);
            }
            catch
            {
                // ignore exceptions, let the validation handle it
            }
        }
        catch (InvalidDataException)
        {
            throw new CorruptArchiveException();
        }
    }

    protected override Task ValidateIfMatchHeaderValue(UploadExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override TicketError? InnerMapDomainException(DomainException exception, UploadExtractSqsLambdaRequest request)
    {
        return ConvertToError(exception, request)?.ToTicketError()
            ?? base.InnerMapDomainException(exception, request);
    }

    private Error? ConvertToError(DomainException exception, UploadExtractSqsLambdaRequest request)
    {
        return exception switch
        {
            UnsupportedMediaTypeException ex => ex.ContentType is not null ? new UnsupportedMediaType(ex.ContentType.Value) : new UnsupportedMediaType(),
            CorruptArchiveException => new Error(ProblemCode.Extract.CorruptArchive),
            ExtractDownloadNotFoundException ex => new ExtractNotFound(ex.DownloadId),
            ExtractRequestMarkedInformativeException => new Error(ProblemCode.Extract.CanNotUploadForInformativeExtract),
            ExtractRequestClosedException => new Error(ProblemCode.Extract.CanNotUploadForClosedExtract),
            CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException => new Error(ProblemCode.Extract.ExtractHasNotBeenDownloaded),
            CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException => new Error(ProblemCode.Extract.CanNotUploadForSupersededDownload),
            _ => null
        };
    }
}
