namespace RoadRegistry.BackOffice.Messages;

public class ImportedRoadSegmentNationalRoadAttribute
{
    public int AttributeId { get; set; }
    public string Number { get; set; }
    public ImportedOriginProperties Origin { get; set; }
}