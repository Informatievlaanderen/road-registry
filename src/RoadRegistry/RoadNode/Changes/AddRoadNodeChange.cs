namespace RoadRegistry.RoadNode.Changes;

using NetTopologySuite.Geometries;
using RoadNetwork;

public sealed record AddRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId TemporaryId { get; init; }
    public RoadNodeId? OriginalId { get; init; }
    public required Point Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
