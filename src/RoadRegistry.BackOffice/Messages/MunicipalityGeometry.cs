namespace RoadRegistry.BackOffice.Messages;

public class MunicipalityGeometry
{
    public Polygon[] MultiPolygon { get; set; }
    public int SpatialReferenceSystemIdentifier { get; set; }
}
