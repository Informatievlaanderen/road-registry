namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using System.IO.Compression;
using Abstractions.Exceptions;
using Abstractions.Extracts.V2;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Exceptions;
using FeatureCompare;
using Framework;
using Hosts;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Requests.Extracts;
using RoadRegistry.Extracts.Schema;
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
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
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
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IBeforeFeatureCompareZipArchiveCleanerFactory beforeFeatureCompareZipArchiveCleanerFactory,
        IZipArchiveFeatureCompareTranslatorFactory featureCompareTranslatorFactory,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing, //new UncompletableTicketing(ticketing), //TODO-pr temp disable want dat zorgt voor rare DI infinite loops
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<UploadExtractSqsLambdaRequestHandler>())
    {
        _extractsDbContext = extractsDbContext;
        _uploadsBlobClient = uploadsBlobClient;
        _beforeFeatureCompareValidatorFactory = beforeFeatureCompareValidatorFactory;
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _beforeFeatureCompareZipArchiveCleanerFactory = beforeFeatureCompareZipArchiveCleanerFactory;
        _featureCompareTranslatorFactory = featureCompareTranslatorFactory;
        _extractUploadFailedEmailClient = extractUploadFailedEmailClient;
    }

    protected override async Task<object> InnerHandle(UploadExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(request.Request.DownloadId);
        var uploadId = new UploadId(request.Request.UploadId);
        var archiveBlob = await _uploadsBlobClient.GetBlobAsync(new BlobName(uploadId), cancellationToken);

        if (!ContentType.TryParse(archiveBlob.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
        {
            throw new UnsupportedMediaTypeException(archiveBlob.ContentType);
        }

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

        var ticketId = new TicketId(request.TicketId);
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
                .WithMessageId(request.TicketId)
                .WithProvenanceData(request.Request.ProvenanceData);
            await _roadNetworkCommandQueue.WriteAsync(command, cancellationToken);

            extractDownload.UploadId = uploadId;
            await _extractsDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidDataException)
        {
            throw new UnsupportedMediaTypeException();
        }
        catch (ZipArchiveValidationException ex)
        {
            if (extractRequest.ExternalRequestId is not null && extractRequest.ExternalRequestId.StartsWith("GRB_"))
            {
                await _extractUploadFailedEmailClient.SendAsync(new(downloadId, extractRequest.Description), cancellationToken);
            }

            throw ex.ToDutchValidationException();
        }
        catch
        {
            if (extractRequest.ExternalRequestId is not null && extractRequest.ExternalRequestId.StartsWith("GRB_"))
            {
                await _extractUploadFailedEmailClient.SendAsync(new(downloadId, extractRequest.Description), cancellationToken);
            }
            throw;
        }

        return new object();
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
            throw new UnsupportedMediaTypeException();
        }
    }

    protected override Task ValidateIfMatchHeaderValue(UploadExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    //TODO-pr TBD: voorlopig zo gedaan zodat de ticketId kan worden doorgegeven aan de command ipv een nieuwe ticket te maken
    // private sealed class UncompletableTicketing : ITicketing
    // {
    //     private readonly ITicketing _ticketing;
    //
    //     public UncompletableTicketing(ITicketing ticketing)
    //     {
    //         _ticketing = ticketing;
    //     }
    //
    //     public Task<Guid> CreateTicket(IDictionary<string, string>? metadata, CancellationToken cancellationToken)
    //     {
    //         throw new NotSupportedException();
    //     }
    //
    //     public Task<IEnumerable<Ticket>> GetAll(CancellationToken cancellationToken)
    //     {
    //         throw new NotSupportedException();
    //     }
    //
    //     public Task<Ticket?> Get(Guid ticketId, CancellationToken cancellationToken)
    //     {
    //         throw new NotSupportedException();
    //     }
    //
    //     public Task Pending(Guid ticketId, CancellationToken cancellationToken)
    //     {
    //         return _ticketing.Pending(ticketId, cancellationToken);
    //     }
    //
    //     public Task Complete(Guid ticketId, TicketResult result, CancellationToken cancellationToken)
    //     {
    //         return Task.CompletedTask;
    //     }
    //
    //     public Task Error(Guid ticketId, TicketError error, CancellationToken cancellationToken)
    //     {
    //         return _ticketing.Error(ticketId, error, cancellationToken);
    //     }
    //
    //     public Task Delete(Guid ticketId, CancellationToken cancellationToken)
    //     {
    //         throw new NotSupportedException();
    //     }
    // }
}
