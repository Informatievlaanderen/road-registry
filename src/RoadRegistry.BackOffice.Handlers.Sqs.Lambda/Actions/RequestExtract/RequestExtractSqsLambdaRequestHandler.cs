namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using TicketingService.Abstractions;

public sealed class RequestExtractSqsLambdaRequestHandler : SqsLambdaHandler<RequestExtractSqsLambdaRequest>
{
    private readonly ExtractRequester _extractRequester;

    public RequestExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractRequester extractRequester,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<RequestExtractSqsLambdaRequestHandler>())
    {
        _extractRequester = extractRequester;
    }

    protected override async Task<object> InnerHandle(RequestExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _extractRequester.BuildExtract(
            new RequestExtractData(request.Request.ExtractRequestId, request.Request.DownloadId, request.Request.Contour.Value, request.Request.Description, request.Request.IsInformative, request.Request.ExternalRequestId),
            new TicketId(request.TicketId), request.Provenance, cancellationToken);

        var downloadId = new DownloadId(request.Request.DownloadId);
        return new RequestExtractResponse(downloadId);
    }

    protected override Task ValidateIfMatchHeaderValue(RequestExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
