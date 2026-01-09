namespace RoadRegistry.RoadSegment.Events.V1.ValueObjects;

public class ImportedRoadSegmentNationalRoadAttribute
{
    public required int AttributeId { get; set; }
    public required string Number { get; set; }
    public required ImportedOriginProperties Origin { get; set; }
}
