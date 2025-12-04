namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.RoadNetwork;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using CommandHandling.Actions.RemoveRoadSegments;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests.RoadNetwork;
using TicketingService.Abstractions;

public sealed class RemoveRoadSegmentsCommandSqsLambdaRequestHandler : SqsLambdaHandler<RemoveRoadSegmentsCommandSqsLambdaRequest>
{
    private readonly RemoveRoadSegmentsCommandHandler _commandHandler;

    public RemoveRoadSegmentsCommandSqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(RemoveRoadSegmentsCommandSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        await _commandHandler.Handle(sqsLambdaRequest.Request, sqsLambdaRequest.Provenance, cancellationToken);

        return new object();
    }
}
