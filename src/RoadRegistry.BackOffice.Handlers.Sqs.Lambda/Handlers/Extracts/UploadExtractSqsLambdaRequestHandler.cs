namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using System.IO.Compression;
using Abstractions.Exceptions;
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

        extractDownload.UploadId = uploadId;
        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        var extractRequest = await _extractsDbContext.ExtractRequests.SingleOrDefaultAsync(x => x.ExtractRequestId == extractDownload.ExtractRequestId && x.CurrentDownloadId == downloadId.ToGuid(), cancellationToken);
        if (extractRequest is null)
        {
            throw new CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException();
        }

        var ticketId = new TicketId(request.Request.TicketId); // NOT the one from the SqsRequest
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
            throw new UnsupportedMediaTypeException();
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
        //TODO-pr TBD: hoe bepalen of het een GRB extract is dat moet worden opgelost via Team DKI?
        //er lijken nog prefixes te bestaat, mss in toekomst nog er bij? dit dan expliciet via endpoint te weten komen
        // op zich, als ExternalRequestId niet null is is het mss al voldoende, het portaal vult die niet op en enkel niet-informatieve kunnen tot hier geraken
        if (extractRequest.ExternalRequestId is not null) // && extractRequest.ExternalRequestId.StartsWith("GRB_"))
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
            throw new UnsupportedMediaTypeException();
        }
    }

    protected override Task ValidateIfMatchHeaderValue(UploadExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
