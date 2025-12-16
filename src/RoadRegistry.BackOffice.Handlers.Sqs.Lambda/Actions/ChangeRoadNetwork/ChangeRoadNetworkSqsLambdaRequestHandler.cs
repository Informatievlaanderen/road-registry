namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.CommandHandling;
using RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;
using RoadRegistry.Hosts;
using TicketingService.Abstractions;

public sealed class ChangeRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadNetworkSqsLambdaRequest>
{
    private readonly ChangeRoadNetworkCommandHandler _commandHandler;

    public ChangeRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ChangeRoadNetworkCommandHandler commandHandler,
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

    protected override async Task<object> InnerHandle(ChangeRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var command = sqsLambdaRequest.Request;

        await Ticketing.Pending(command.TicketId, cancellationToken);

        var changeResult = await _commandHandler.Handle(command, sqsLambdaRequest.Provenance, cancellationToken);

        var hasError = changeResult.Problems.HasError();
        if (hasError)
        {
            //TODO-pr fill in errorcontext met juiste identifier, bvb WegsegmentId, WegknoopId, OngelijkGrondseKruisingId
            var errors = changeResult.Problems
                .Select(problem => problem.Translate().ToTicketError())
                .ToArray();

            await Ticketing.Error(command.TicketId, new TicketError(errors), cancellationToken);
        }
        else
        {
            await Ticketing.Complete(command.TicketId, new TicketResult(new
            {
                Changes = new
                {
                    RoadNodes = new
                    {
                        Added = changeResult.Changes.RoadNodes.Added.Select(x => x.ToInt32()).ToList(),
                        Modified = changeResult.Changes.RoadNodes.Modified.Select(x => x.ToInt32()).ToList(),
                        Removed = changeResult.Changes.RoadNodes.Removed.Select(x => x.ToInt32()).ToList()
                    },
                    RoadSegments = new
                    {
                        Added = changeResult.Changes.RoadSegments.Added.Select(x => x.ToInt32()).ToList(),
                        Modified = changeResult.Changes.RoadSegments.Modified.Select(x => x.ToInt32()).ToList(),
                        Removed = changeResult.Changes.RoadSegments.Removed.Select(x => x.ToInt32()).ToList()
                    },
                    GradeSeparatedJunctions = new
                    {
                        Added = changeResult.Changes.GradeSeparatedJunctions.Added.Select(x => x.ToInt32()).ToList(),
                        Modified = changeResult.Changes.GradeSeparatedJunctions.Modified.Select(x => x.ToInt32()).ToList(),
                        Removed = changeResult.Changes.GradeSeparatedJunctions.Removed.Select(x => x.ToInt32()).ToList()
                    }
                }
            }), cancellationToken);
        }

        return new object();
    }
}
