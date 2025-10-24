namespace RoadRegistry.RoadNetwork.ValueObjects;

public sealed class GeometryObject
{
    public int SRID { get; }
    public string WKT { get; }

    public GeometryObject(int srid, string wkt)
    {
        SRID = srid;
        WKT = wkt;
    }
}
