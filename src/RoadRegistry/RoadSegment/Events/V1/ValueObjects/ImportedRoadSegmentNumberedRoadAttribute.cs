namespace RoadRegistry.RoadSegment.Events.V1.ValueObjects;

public class ImportedRoadSegmentNumberedRoadAttribute
{
    public required int AttributeId { get; set; }
    public required string Direction { get; set; }
    public required string Number { get; set; }
    public required int Ordinal { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
}
