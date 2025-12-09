namespace RoadRegistry.RoadSegment.Events.V1.ValueObjects;

public class RoadSegmentSurfaceAttributes
{
    public required int AsOfGeometryVersion { get; set; }
    public required int AttributeId { get; set; }
    public required decimal FromPosition { get; set; }
    public required decimal ToPosition { get; set; }
    public required string Type { get; set; }
}
