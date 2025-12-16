namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.CommandHandling.Actions.RemoveRoadSegments;
using RoadRegistry.Hosts;
using TicketingService.Abstractions;

public sealed class RemoveRoadSegmentsSqsLambdaRequestHandler : SqsLambdaHandler<RemoveRoadSegmentsSqsLambdaRequest>
{
    private readonly RemoveRoadSegmentsCommandHandler _commandHandler;

    public RemoveRoadSegmentsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        RemoveRoadSegmentsCommandHandler commandHandler,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory)
    {
        _commandHandler = commandHandler;
    }

    protected override async Task<object> InnerHandle(RemoveRoadSegmentsSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        await _commandHandler.Handle(sqsLambdaRequest.Request, sqsLambdaRequest.Provenance, cancellationToken);

        return new object();
    }
}
