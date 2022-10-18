namespace RoadRegistry.BackOffice.Messages;

public class RoadSegmentWidthAttributes
{
    public int AsOfGeometryVersion { get; set; }
    public int AttributeId { get; set; }
    public decimal FromPosition { get; set; }
    public decimal ToPosition { get; set; }
    public int Width { get; set; }
}
