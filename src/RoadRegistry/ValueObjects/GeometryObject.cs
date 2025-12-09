namespace RoadRegistry.ValueObjects;

using NetTopologySuite.Geometries;

public record GeometryObject
{
    public int SRID { get; }
    public string WKT { get; }

    public GeometryObject(int srid, string wkt)
    {
        SRID = srid;
        WKT = wkt;
    }
    protected GeometryObject(Geometry geometry)
        : this(geometry.SRID, geometry.AsText())
    {
    }
}
