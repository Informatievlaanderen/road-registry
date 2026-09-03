namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenChangingRoadSegmentStatusV2;

using System.Collections.Generic;
using System.Linq;
using Marten;
using NetTopologySuite.Geometries;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// Scoping decides which segments the domain gets to see, and how far the handler scopes is the one thing that differs
// per kind of status change - so the fake honours it the way the real repository does.
internal sealed class StatusChangeFakeRoadNetworkRepository : IRoadNetworkRepository
{
    private readonly RoadNetworkRepository _real;
    private readonly IReadOnlyList<RoadNode> _nodes;
    private readonly IReadOnlyList<RoadSegment> _segments;

    public StatusChangeFakeRoadNetworkRepository(IDocumentStore store, IReadOnlyList<RoadNode> nodes, IReadOnlyList<RoadSegment> segments)
    {
        _real = new RoadNetworkRepository(store);
        _nodes = nodes;
        _segments = segments;
    }

    public Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
    {
        var scoped = _segments
            .Where(x => ids is not null && ids.RoadSegmentIds.Contains(x.RoadSegmentId)
                        || geometry is not null && x.Geometry.Value.Intersects(geometry))
            .ToList();

        return Task.FromResult(ToIds(scoped));
    }

    public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        var nodeIds = _segments
            .Where(x => roadSegmentIds.Contains(x.RoadSegmentId))
            .SelectMany(NodeIdsOf)
            .ToHashSet();

        var connected = _segments
            .Where(x => roadSegmentIds.Contains(x.RoadSegmentId) || NodeIdsOf(x).Any(nodeIds.Contains))
            .ToList();

        return Task.FromResult(ToIds(connected));
    }

    public Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
    {
        return Task.FromResult(new ScopedRoadNetwork(roadNetworkId,
            _nodes.Where(x => ids.RoadNodeIds.Contains(x.RoadNodeId)).ToArray(),
            _segments.Where(x => ids.RoadSegmentIds.Contains(x.RoadSegmentId)).ToArray(),
            [],
            []));
    }

    private static IEnumerable<RoadNodeId> NodeIdsOf(RoadSegment segment)
    {
        return new[] { segment.StartNodeId, segment.EndNodeId }
            .Where(x => x is not null)
            .Select(x => x!.Value);
    }

    private RoadNetworkIds ToIds(IReadOnlyCollection<RoadSegment> segments)
    {
        return new RoadNetworkIds(
            segments.SelectMany(NodeIdsOf).Distinct().ToArray(),
            segments.Select(x => x.RoadSegmentId).Distinct().ToArray(),
            [],
            []);
    }

    public void Save(IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName)
        => _real.Save(session, roadNetwork, commandName);

    public Task<RoadNetworkIds> GetUnderlyingIdsForExtract(IDocumentSession session, Geometry geometry)
        => throw new NotImplementedException();

    public Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
