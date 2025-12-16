namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using Abstractions.Extracts.V2;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests.Extracts;
using TicketingService.Abstractions;

public sealed class RequestExtractSqsLambdaRequestHandler : SqsLambdaHandler<RequestExtractSqsLambdaRequest>
{
    private readonly ExtractsEngine _extractsEngine;

    public RequestExtractSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ExtractsEngine extractsEngine,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory.CreateLogger<RequestExtractSqsLambdaRequestHandler>())
    {
        _extractsEngine = extractsEngine;
    }

    protected override async Task<object> InnerHandle(RequestExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _extractsEngine.BuildExtract(request.Request, new TicketId(request.TicketId), request.Provenance, cancellationToken);

        var downloadId = new DownloadId(request.Request.DownloadId);
        return new RequestExtractResponse(downloadId);
    }

    protected override Task ValidateIfMatchHeaderValue(RequestExtractSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
