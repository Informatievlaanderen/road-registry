namespace RoadRegistry.RoadSegment.Events;

public class RoadSegmentSurfaceAttribute
{
    public required int AttributeId { get; set; }
    public required decimal FromPosition { get; set; }
    public required decimal ToPosition { get; set; }
    public required string Type { get; set; }
}
