namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeGradeSeparatedJunctionAttributes;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using TicketingService.Abstractions;

public sealed class ChangeGradeSeparatedJunctionAttributesV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeGradeSeparatedJunctionAttributesV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;

    public ChangeGradeSeparatedJunctionAttributesV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            store,
            loggerFactory)
    {
        _roadNetworkRepository = roadNetworkRepository;
    }

    protected override async Task<object> InnerHandle(ChangeGradeSeparatedJunctionAttributesV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(ChangeGradeSeparatedJunctionAttributesV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetwork = await Load(session, command.GradeSeparatedJunctionId, scopedRoadNetworkId);

            var result = roadNetwork.ModifyGradeSeparatedJunctionAttributes(
                command.GradeSeparatedJunctionId,
                command.LowerRoadSegmentId,
                command.UpperRoadSegmentId,
                command.Type,
                command.ProvenanceData.ToProvenance(),
                Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    // Seeded from the grade separated junction, which brings in the two road segments it is about.
    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, GradeSeparatedJunctionId gradeSeparatedJunctionId, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], [], [gradeSeparatedJunctionId], []));
        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }
}
