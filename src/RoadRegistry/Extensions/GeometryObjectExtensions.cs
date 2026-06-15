namespace RoadRegistry.Extensions;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;

public static class GeometryObjectExtensions
{
    public static RoadNodeGeometry EnsureLambert08(this RoadNodeGeometry geometry)
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
    public static RoadNodeGeometry EnsureLambert72(this RoadNodeGeometry geometry)
    {
        if (geometry.Value.IsLambert72())
        {
            return geometry;
        }

        if (geometry.Value.IsLambert08())
        {
            return RoadNodeGeometry.Create(geometry.Value.TransformFromLambert08To72());
        }

        throw new InvalidCastException($"Geometry SRID {geometry.SRID} is not Lambert72 or Lambert08");
    }

    public static RoadSegmentGeometry EnsureLambert08(this RoadSegmentGeometry geometry)
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
    public static RoadSegmentGeometry EnsureLambert72(this RoadSegmentGeometry geometry)
    {
        if (geometry.Value.IsLambert72())
        {
            return geometry;
        }

        if (geometry.Value.IsLambert08())
        {
            return RoadSegmentGeometry.Create(geometry.Value.TransformFromLambert08To72());
        }

        throw new InvalidCastException($"Geometry SRID {geometry.SRID} is not Lambert72 or Lambert08");
    }

    public static ExtractGeometry EnsureLambert08(this ExtractGeometry geometry)
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
