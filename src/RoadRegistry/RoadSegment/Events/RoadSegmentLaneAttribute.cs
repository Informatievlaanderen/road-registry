namespace RoadRegistry.RoadSegment.Events;

public class RoadSegmentLaneAttribute
{
    public required int AttributeId { get; set; }
    public required decimal FromPosition { get; set; }
    public required decimal ToPosition { get; set; }
    public required int Count { get; set; }
    public required string Direction { get; set; }
}
