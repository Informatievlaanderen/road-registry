namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentStatus;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class ChangeRoadSegmentStatusV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadSegmentStatusV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;

    public ChangeRoadSegmentStatusV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        ExtractsDbContext extractsDbContext,
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
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentStatusV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(ChangeRoadSegmentStatusV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var ids = await GetUnderlyingIds(session, command, cancellationToken);
            var roadNetwork = await _roadNetworkRepository.Load(session, ids, scopedRoadNetworkId);

            var problems = await ValidateInwinningIsCompleted(roadNetwork, command.RoadSegmentId, cancellationToken);
            problems.ThrowIfError();

            var result = roadNetwork.ChangeRoadSegmentStatus(
                command.StatusChange,
                command.RoadSegmentId,
                command.MayModifyMeasuredRoadSegments,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    // How far around the segment the domain has to be able to see depends on what the status change does to the
    // topology.
    private async Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, ChangeRoadSegmentStatusV2SqsRequest command, CancellationToken cancellationToken)
    {
        if (command.StatusChange.Connects)
        {
            // The segments this one is about to hook onto share no road node with it yet - that is the whole point of
            // connecting it - so the scope is taken from the segment's own geometry. Its own id is passed along as
            // well, because a segment outside the network carries no nodes and so has nothing to be found through.
            var roadSegment = await session.LoadAsync(command.RoadSegmentId, cancellationToken);
            if (roadSegment is null)
            {
                var roadSegmentContext = Problems.WithContext(command.RoadSegmentId);
                throw new RoadRegistryProblemsException(roadSegmentContext + new RoadSegmentNotFound());
            }

            var geometry = roadSegment.Geometry.Value.Buffer(Distances.RoadSegmentRealizeMaximumDistanceToRoadNode + 0.5 /*buffer to get connected segments*/);
            return await _roadNetworkRepository.GetUnderlyingIds(session, geometry);
        }

        if (command.StatusChange.Disconnects)
        {
            // A realized segment is knotted into the network, so everything this action touches is reachable through
            // its road nodes: the segment itself, the nodes at either end, the segments hanging off them - which is
            // what decides whether a node survives - and the junctions it takes part in.
            return await _roadNetworkRepository.GetUnderlyingIdsWithConnectedSegments(session, [command.RoadSegmentId]);
        }

        // The segment is outside the network before and after, so nothing around it is touched or consulted.
        return await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], [command.RoadSegmentId], [], []));
    }

    // A road segment may only be edited once its inwinning is done: one that is still being collected, or that is not
    // being collected at all, is not ours to change yet.
    private async Task<Problems> ValidateInwinningIsCompleted(ScopedRoadNetwork roadNetwork, RoadSegmentId roadSegmentId, CancellationToken cancellationToken)
    {
        // An identifier the road network does not know is the domain's to report as not found, and 'nietGestart' cannot
        // tell "not being collected" from "does not exist" - so checking it here would answer a missing road segment
        // with the wrong problem entirely.
        if (!roadNetwork.RoadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            return Problems.None;
        }

        var inwinningsstatus = await _extractsDbContext.GetInwinningsstatus([roadSegmentId], cancellationToken);

        return inwinningsstatus
            .Where(x => x.Value != Inwinningsstatus.Compleet)
            .Aggregate(Problems.None, (problems, x) => problems + new RoadSegmentNotCompletedInwinning(x.Key));
    }
}
