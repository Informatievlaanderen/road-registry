namespace RoadRegistry.RoadSegment.Events.V1.ValueObjects;

public class ImportedRoadSegmentWidthAttribute
{
    public required int AsOfGeometryVersion { get; set; }
    public required int AttributeId { get; set; }
    public required decimal FromPosition { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
    public required decimal ToPosition { get; set; }
    public required int Width { get; set; }
}
