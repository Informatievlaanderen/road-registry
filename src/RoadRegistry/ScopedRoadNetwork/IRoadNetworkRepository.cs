namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using NetTopologySuite.Geometries;
using ValueObjects;

public interface IRoadNetworkRepository
{
    Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry geometry);
    Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds);
    Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, RoadNetworkId roadNetworkId);
    Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken);
}

public sealed record RoadNetworkIds(IReadOnlyCollection<RoadNodeId> RoadNodeIds, IReadOnlyCollection<RoadSegmentId> RoadSegmentIds, IReadOnlyCollection<GradeSeparatedJunctionId> GradeSeparatedJunctionIds);
