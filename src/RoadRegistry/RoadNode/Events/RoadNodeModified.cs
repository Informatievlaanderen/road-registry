namespace RoadRegistry.RoadNode.Events;

public record RoadNodeModified
{
    public required RoadNodeId RoadNodeId { get; init; }
    public GeometryObject? Geometry { get; init; }
    public RoadNodeType? Type { get; init; }
}
