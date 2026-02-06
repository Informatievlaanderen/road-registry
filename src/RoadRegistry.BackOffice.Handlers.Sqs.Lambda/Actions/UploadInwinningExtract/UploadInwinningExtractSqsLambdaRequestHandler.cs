namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadInwinningExtract;

using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;
using UploadExtract;

public sealed class UploadInwinningExtractSqsLambdaRequestHandler : SqsLambdaHandler<UploadInwinningExtractSqsLambdaRequest>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IExtractUploader _extractUploader;
    private readonly IMediator _mediator;

    public UploadInwinningExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsDbContext extractsDbContext,
        IExtractUploader extractUploader,
        IMediator mediator,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<UploadExtractSqsLambdaRequestV2Handler>(),
            TicketingBehavior.Error)
    {
        _extractsDbContext = extractsDbContext;
        _extractUploader = extractUploader;
        _mediator = mediator;
    }

    protected override async Task<object> InnerHandle(UploadInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var inwinningszone = await _extractsDbContext.Inwinningszones.SingleOrDefaultAsync(x => x.DownloadId == request.Request.DownloadId.ToGuid(), cancellationToken);
            if (inwinningszone is null)
            {
                throw new InvalidOperationException($"No Inwinningszone found for {request.Request.DownloadId}");
            }

            if (inwinningszone.Completed)
            {
                throw new InwinningszoneCompletedException(request.Request.DownloadId);
            }

            var ticketId = new TicketId(request.TicketId);
            var translatedChanges = await _extractUploader.ProcessUploadAndDetectChanges(request.Request.DownloadId, request.Request.UploadId, ticketId, ZipArchiveMetadata.Empty.WithInwinning(), cancellationToken);

            var migrateDryRunRoadNetworkSqsRequest = new MigrateDryRunRoadNetworkSqsRequest
            {
                TicketId = ticketId,
                ProvenanceData = new ProvenanceData(request.Provenance),
                MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
                {
                    TicketId = ticketId,
                    DownloadId = request.Request.DownloadId,
                    UploadId = request.Request.UploadId,
                    Changes = translatedChanges.Select(ChangeRoadNetworkItem.Create).ToList(),
                    ProvenanceData = new ProvenanceData(request.Provenance)
                }
            };
            await _mediator.Send(migrateDryRunRoadNetworkSqsRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }

        return new object();
    }

    protected override Task ValidateIfMatchHeaderValue(UploadInwinningExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override TicketError? InnerMapDomainException(DomainException exception, UploadInwinningExtractSqsLambdaRequest request)
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
            InwinningszoneCompletedException => new Error(ProblemCode.Extract.InwinningszoneCompleted),
            CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException => new Error(ProblemCode.Extract.ExtractHasNotBeenDownloaded),
            CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException => new Error(ProblemCode.Extract.CanNotUploadForSupersededDownload),
            _ => null
        };
    }
}
