namespace RoadRegistry.BackOffice.Messages;

public class RoadNetworkExtractGeometry
{
    public Polygon[] MultiPolygon { get; set; }
    public Polygon Polygon { get; set; }
    public string WKT { get; set; }
    public int SpatialReferenceSystemIdentifier { get; set; }
}
