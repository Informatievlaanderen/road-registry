namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using System.IO.Compression;
using Abstractions.Exceptions;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Exceptions;
using FeatureCompare.V3;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Requests.Extracts;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.RoadNetwork.ValueObjects;
using Sqs.RoadNetwork;
using TicketingService.Abstractions;
using ValueObjects.ProblemCodes;
using ValueObjects.Problems;

public sealed class UploadExtractSqsLambdaRequestV2Handler : SqsLambdaHandler<UploadExtractSqsLambdaRequestV2>
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
    private readonly IMediator _mediator;

    public UploadExtractSqsLambdaRequestV2Handler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        IZipArchiveFeatureCompareTranslator featureCompareTranslator,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        IMediator mediator,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<UploadExtractSqsLambdaRequestV2Handler>())
    {
        _extractsDbContext = extractsDbContext;
        _uploadsBlobClient = uploadsBlobClient;
        _featureCompareTranslator = featureCompareTranslator;
        _extractUploadFailedEmailClient = extractUploadFailedEmailClient;
        _mediator = mediator;
    }

    protected override async Task<object> InnerHandle(UploadExtractSqsLambdaRequestV2 request, CancellationToken cancellationToken)
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

    private async Task<object> InnerHandleForCustomTicketId(UploadExtractSqsLambdaRequestV2 request, TicketId ticketId, CancellationToken cancellationToken)
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

        try
        {
            await using var archiveBlobStream = await archiveBlob.OpenAsync(cancellationToken);
            using var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, true);

            var translatedChanges = await _featureCompareTranslator.TranslateAsync(archive, ZipArchiveMetadata.Empty.WithDownloadId(downloadId), cancellationToken);

            var changeRoadNetworkSqsRequest = new ChangeRoadNetworkCommandSqsRequest
            {
                Request = translatedChanges.ToChangeRoadNetworkCommand(downloadId, ticketId),
                ProvenanceData = new ProvenanceData(request.Provenance)
            };
            await _mediator.Send(changeRoadNetworkSqsRequest, cancellationToken);
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

    protected override Task ValidateIfMatchHeaderValue(UploadExtractSqsLambdaRequestV2 request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override TicketError? InnerMapDomainException(DomainException exception, UploadExtractSqsLambdaRequestV2 request)
    {
        return ConvertToDomainError(exception)?.ToTicketError()
            ?? base.InnerMapDomainException(exception, request);
    }

    private Error? ConvertToDomainError(DomainException exception)
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
