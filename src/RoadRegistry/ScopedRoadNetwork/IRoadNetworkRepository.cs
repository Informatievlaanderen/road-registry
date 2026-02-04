namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using NetTopologySuite.Geometries;
using ValueObjects;

public interface IRoadNetworkRepository
{
    Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null, bool onlyV2 = false);
    Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds);
    Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId);
    Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken);
}

public sealed record RoadNetworkIds(IReadOnlyCollection<RoadNodeId> RoadNodeIds, IReadOnlyCollection<RoadSegmentId> RoadSegmentIds, IReadOnlyCollection<GradeSeparatedJunctionId> GradeSeparatedJunctionIds)
{
    public bool IsEmpty => RoadNodeIds.Count == 0 && RoadSegmentIds.Count == 0 && GradeSeparatedJunctionIds.Count == 0;
}
