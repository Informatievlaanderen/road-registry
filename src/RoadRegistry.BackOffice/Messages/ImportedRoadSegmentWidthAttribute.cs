namespace RoadRegistry.BackOffice.Messages;

public class ImportedRoadSegmentWidthAttribute
{
    public int AsOfGeometryVersion { get; set; }
    public int AttributeId { get; set; }
    public decimal FromPosition { get; set; }
    public ImportedOriginProperties Origin { get; set; }
    public decimal ToPosition { get; set; }
    public int Width { get; set; }
}
