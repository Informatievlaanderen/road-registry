namespace RoadRegistry.ValueObjects;

using NetTopologySuite.Geometries;

public record ExtractGeometry : GeometryObject
{
    public ExtractGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private ExtractGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static ExtractGeometry Create(Geometry geometry)
    {
        return new ExtractGeometry(geometry);
    }
}
