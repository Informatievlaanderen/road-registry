namespace RoadRegistry.ValueObjects;

using NetTopologySuite.Geometries;

public record RoadNodeGeometry : GeometryObject
{
    public RoadNodeGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private RoadNodeGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static RoadNodeGeometry Create(Geometry geometry)
    {
        return new RoadNodeGeometry(geometry);
    }
}
