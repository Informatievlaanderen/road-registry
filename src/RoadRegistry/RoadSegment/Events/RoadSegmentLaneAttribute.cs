namespace RoadRegistry.RoadSegment.Events;

public class RoadSegmentLaneAttribute
{
    public required int AttributeId { get; init; }
    public required decimal FromPosition { get; init; }
    public required decimal ToPosition { get; init; }
    public required int Count { get; init; }
    public required string Direction { get; init; }
}
