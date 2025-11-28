namespace RoadRegistry.ValueObjects;

public sealed record GeometryObject
{
    public int SRID { get; }
    public string WKT { get; }

    public GeometryObject(int srid, string wkt)
    {
        SRID = srid;
        WKT = wkt;
    }
}
