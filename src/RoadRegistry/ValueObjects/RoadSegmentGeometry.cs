namespace RoadRegistry.ValueObjects;

using NetTopologySuite.Geometries;

public record RoadSegmentGeometry : GeometryObject
{
    public RoadSegmentGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private RoadSegmentGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static RoadSegmentGeometry Create(Geometry geometry)
    {
        return new RoadSegmentGeometry(geometry);
    }
}
