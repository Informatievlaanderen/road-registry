namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using NetTopologySuite.Geometries;
using ValueObjects;

public interface IRoadNetworkRepository
{
    // The road network a geometry and ids put in scope: the road segments intersecting the geometry or named, or
    // reached through a road node or a junction named, with their road nodes and junctions - loaded in one go.
    Task<ScopedRoadNetwork> LoadUnderlying(IDocumentSession session, ScopedRoadNetworkId roadNetworkId, Geometry? geometry = null, RoadNetworkIds? ids = null);

    // The road segments named and every road segment meeting one of them at a road node, with their road nodes and
    // junctions - loaded in one go.
    Task<ScopedRoadNetwork> LoadWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId);

    // The extract documents of what an extract of the geometry is made of, loaded in one go. The documents are
    // taken by their id, which has to be the id of the road node, road segment or grade separated junction they are
    // about.
    Task<RoadNetworkExtractDocuments<TRoadNode, TRoadSegment, TGradeSeparatedJunction>> LoadForExtract<TRoadNode, TRoadSegment, TGradeSeparatedJunction>(IDocumentSession session, Geometry geometry)
        where TRoadNode : class
        where TRoadSegment : class
        where TGradeSeparatedJunction : class;

    // Exactly the entities named.
    Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId);
    Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken);
    void Save(IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName);
}

public sealed record RoadNetworkExtractDocuments<TRoadNode, TRoadSegment, TGradeSeparatedJunction>(
    IReadOnlyList<TRoadNode> RoadNodes,
    IReadOnlyList<TRoadSegment> RoadSegments,
    IReadOnlyList<TGradeSeparatedJunction> GradeSeparatedJunctions);

public sealed record RoadNetworkIds(
    IReadOnlyCollection<RoadNodeId> RoadNodeIds,
    IReadOnlyCollection<RoadSegmentId> RoadSegmentIds,
    IReadOnlyCollection<GradeSeparatedJunctionId> GradeSeparatedJunctionIds,
    IReadOnlyCollection<GradeJunctionId> GradeJunctionIds)
{
    public bool IsEmpty => RoadNodeIds.Count == 0 && RoadSegmentIds.Count == 0 && GradeSeparatedJunctionIds.Count == 0 && GradeJunctionIds.Count == 0;
}
