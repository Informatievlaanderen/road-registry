namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadNode;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using TicketingService.Abstractions;

public sealed class RemoveRoadNodeV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<RemoveRoadNodeV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public RemoveRoadNodeV2SqsLambdaRequestHandler(
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

    protected override async Task<object> InnerHandle(RemoveRoadNodeV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(RemoveRoadNodeV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetwork = await Load(session, command.RoadNodeId, scopedRoadNetworkId);

            var result = roadNetwork.RemoveRoadNodeByMergingRoadSegments(
                command.RoadNodeId,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        return await GetSummaryOfLastChange(scopedRoadNetworkId, cancellationToken);
    }

    // Seeded from the road node, which brings in the road segments hanging off it: the roads that are about to become
    // one. That is not enough on its own. The merged road runs the length of both, and the merge is refused when it
    // would run into another road twice (VAL-8) - a check that asks the road network what else lies along it. Seeded
    // from the road node alone the road network holds nothing but the roads at that node, every one of which the check
    // passes over, so it had nothing to compare against and never reported anything (GAWR-7824). The same blind spot
    // left the crossings that are recorded afterwards to be worked out from a part of the picture.
    //
    // So a second pass loads everything the roads that are becoming one touch. Their geometry is what says where to
    // look and is only known once they are loaded, which is what the first pass is for - a handful of road segments.
    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNodeId roadNodeId, ScopedRoadNetworkId roadNetworkId)
    {
        var connectedIds = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([roadNodeId], [], [], []));

        var alongTheMergedRoad = await GetAreaAroundRoadSegments(session, connectedIds.RoadSegmentIds);

        var ids = alongTheMergedRoad is not null
            ? await _roadNetworkRepository.GetUnderlyingIds(session, alongTheMergedRoad, ids: connectedIds)
            : connectedIds;

        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }

    // How far around the roads that are becoming one to look. Slack rather than a threshold: the checks that follow
    // work to a tolerance of their own, so what has to be loaded is everything that could matter to them, and what is
    // loaded beyond that costs a row and nothing more.
    private static readonly double LookAroundMergedRoad = Distances.TooClose;

    // The ground the road segments hanging off the road node cover, as one area to look around in. Null when there
    // are none to speak of: a road node without roads is not one this action can do anything with, and the domain
    // says so for itself.
    private static async Task<Geometry?> GetAreaAroundRoadSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        if (roadSegmentIds.Count == 0)
        {
            return null;
        }

        var geometries = (await session.LoadManyAsync(roadSegmentIds))
            .Where(x => !x.IsRemoved)
            .Select(x => x.Geometry)
            .ToArray();

        if (geometries.Length == 0)
        {
            return null;
        }

        // An area rather than the roads themselves. The lookup asks what intersects what it is given, and a road only
        // intersects a line it actually touches - so a road ending just short of the merged road, or running along it
        // close enough for the checks to have something to say, would be left out of the road network and then
        // silently not count. Every other action that looks around itself hands over an area for the same reason.
        //
        // Unioned rather than collected into a multi polygon: the buffers overlap around the road node the roads meet
        // at, and a multi polygon whose parts overlap is invalid - every overlay operation on it, here and in PostGIS,
        // throws a side location conflict.
        return geometries
            .Select(x => x.Value.Buffer(LookAroundMergedRoad))
            .Aggregate((left, right) => left.Union(right))
            .WithSrid(geometries[0].SRID);
    }
}
