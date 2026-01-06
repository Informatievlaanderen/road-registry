namespace RoadRegistry.RoadNode.Changes;

using NetTopologySuite.Geometries;
using RoadNetwork;

public sealed record MigrateRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId RoadNodeId { get; init; }
    public required Point Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
