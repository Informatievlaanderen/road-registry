namespace RoadRegistry.BackOffice.Messages;

public class ImportedRoadSegmentSurfaceAttribute
{
    public int AsOfGeometryVersion { get; set; }
    public int AttributeId { get; set; }
    public decimal FromPosition { get; set; }
    public ImportedOriginProperties Origin { get; set; }
    public decimal ToPosition { get; set; }
    public string Type { get; set; }
}
