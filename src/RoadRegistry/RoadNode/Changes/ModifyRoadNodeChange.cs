namespace RoadRegistry.RoadNode.Changes;

using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice;

public sealed record ModifyRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId Id { get; init; }
    public required Point? Geometry { get; init; }
    public required RoadNodeType? Type { get; init; }
}
