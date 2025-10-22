namespace RoadRegistry.RoadSegment.Events;

public class RoadSegmentWidthAttribute
{
    public required int AttributeId { get; set; }
    public required decimal FromPosition { get; set; }
    public required decimal ToPosition { get; set; }
    public required int Width { get; set; }
}
