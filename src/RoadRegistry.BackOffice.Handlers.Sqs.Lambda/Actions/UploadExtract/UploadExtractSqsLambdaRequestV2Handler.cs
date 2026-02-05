namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;

using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class UploadExtractSqsLambdaRequestV2Handler : SqsLambdaHandler<UploadExtractSqsLambdaRequestV2>
{
    private readonly IExtractUploader _extractUploader;
    private readonly IMediator _mediator;

    public UploadExtractSqsLambdaRequestV2Handler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
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
        _extractUploader = extractUploader;
        _mediator = mediator;
    }

    protected override async Task<object> InnerHandle(UploadExtractSqsLambdaRequestV2 request, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(request.TicketId);
        var translatedChanges = await _extractUploader.ProcessUploadAndDetectChanges(request.Request.DownloadId, request.Request.UploadId, ticketId, ZipArchiveMetadata.Empty, cancellationToken);

        var changeRoadNetworkSqsRequest = new ChangeRoadNetworkSqsRequest
        {
            TicketId = ticketId,
            DownloadId = request.Request.DownloadId,
            UploadId = request.Request.UploadId,
            Changes = translatedChanges.Select(ChangeRoadNetworkItem.Create).ToList(),
            SendFailedEmail = request.Request.SendFailedEmail,
            ProvenanceData = new ProvenanceData(request.Provenance)
        };
        await _mediator.Send(changeRoadNetworkSqsRequest, cancellationToken);

        return new object();
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
