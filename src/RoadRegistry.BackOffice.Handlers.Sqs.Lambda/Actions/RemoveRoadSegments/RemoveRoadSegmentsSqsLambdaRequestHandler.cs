namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Hosts;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class RemoveRoadSegmentsSqsLambdaRequestHandler : MartenSqsLambdaHandler<RemoveRoadSegmentsSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public RemoveRoadSegmentsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
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
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    protected override async Task<object> InnerHandle(RemoveRoadSegmentsSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(RemoveRoadSegmentsSqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetwork = await Load(session, command.RoadSegmentIds, scopedRoadNetworkId);

            roadNetwork.RemoveRoadSegments(command.RoadSegmentIds, _roadNetworkIdGenerator, command.ProvenanceData.ToProvenance());

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        // Read back rather than carried out of the domain: a request that removed nothing raises no change at all, and
        // then there is no summary to report - the same answer the status changes give.
        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIdsWithConnectedSegments(session, roadSegmentIds);
        return await _roadNetworkRepository.Load(
            session,
            ids,
            roadNetworkId
        );
    }
}
