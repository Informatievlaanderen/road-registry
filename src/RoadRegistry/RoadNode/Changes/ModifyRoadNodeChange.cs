namespace RoadRegistry.RoadNode.Changes;

using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice;

public sealed record ModifyRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }
    public Point? Geometry { get; init; }
    public RoadNodeType? Type { get; init; }
}
