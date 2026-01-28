namespace RoadRegistry.Infrastructure;

public static class GeometryObjectExtensions
{
    public static RoadNodeGeometry ToLambert08(this RoadNodeGeometry geometry)
    {
        if (geometry.Value.IsLambert08())
        {
            return geometry;
        }

        if (geometry.Value.IsLambert72())
        {
            return RoadNodeGeometry.Create(geometry.Value.TransformFromLambert72To08());
        }

        throw new InvalidCastException($"Geometry SRID {geometry.SRID} is not Lambert72 or Lambert08");
    }

    public static RoadSegmentGeometry ToLambert08(this RoadSegmentGeometry geometry)
    {
        if (geometry.Value.IsLambert08())
        {
            return geometry;
        }

        if (geometry.Value.IsLambert72())
        {
            return RoadSegmentGeometry.Create(geometry.Value.TransformFromLambert72To08());
        }

        throw new InvalidCastException($"Geometry SRID {geometry.SRID} is not Lambert72 or Lambert08");
    }

    public static ExtractGeometry ToLambert08(this ExtractGeometry geometry)
    {
        if (geometry.Value.IsLambert08())
        {
            return geometry;
        }

        if (geometry.Value.IsLambert72())
        {
            return ExtractGeometry.Create(geometry.Value.TransformFromLambert72To08());
        }

        throw new InvalidCastException($"Geometry SRID {geometry.SRID} is not Lambert72 or Lambert08");
    }
}
