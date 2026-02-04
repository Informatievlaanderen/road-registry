namespace RoadRegistry.RoadSegment.Events.V1.ValueObjects;

public class ImportedRoadSegmentLaneAttribute
{
    public required int AsOfGeometryVersion { get; set; }
    public required int AttributeId { get; set; }
    public required int Count { get; set; }
    public required string Direction { get; set; }
    public required double FromPosition { get; set; }
    public required double ToPosition { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
}
