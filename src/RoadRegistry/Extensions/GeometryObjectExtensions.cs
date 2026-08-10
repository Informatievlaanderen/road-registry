namespace RoadRegistry.Extensions;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

public static class GeometryObjectExtensions
{
    // Rebuilds a geometry with X/Y-only coordinates (no Z, no M), preserving SRID and structure. Road v2 domain
    // geometries are 2D, but a Z or M ordinate can still arrive from the outside (legacy V1 geometries, GRB
    // imports), and the read-model target databases (WmsWfsV2, PBS SQL Server) must store plain 2D geometries.
    // A fresh CoordinateArraySequence-based factory is used on purpose so no Z/M slots are re-introduced.
    public static Geometry Force2D(this Geometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var factory = new GeometryFactory(geometry.PrecisionModel, geometry.SRID, CoordinateArraySequenceFactory.Instance);
        return Rebuild2D(geometry, factory);
    }

    private static Geometry Rebuild2D(Geometry geometry, GeometryFactory factory)
    {
        return geometry switch
        {
            Point point => point.IsEmpty ? factory.CreatePoint() : factory.CreatePoint(To2D(point.Coordinate)),
            LineString lineString => factory.CreateLineString(lineString.Coordinates.Select(To2D).ToArray()),
            MultiLineString multiLineString => factory.CreateMultiLineString(
                multiLineString.Geometries.Cast<LineString>().Select(x => (LineString)Rebuild2D(x, factory)).ToArray()),
            MultiPoint multiPoint => factory.CreateMultiPoint(
                multiPoint.Geometries.Cast<Point>().Select(x => (Point)Rebuild2D(x, factory)).ToArray()),
            _ => throw new NotSupportedException($"{nameof(Force2D)} does not support geometry type '{geometry.GeometryType}'.")
        };
    }

    private static Coordinate To2D(Coordinate coordinate)
    {
        return new Coordinate(coordinate.X, coordinate.Y);
    }

    // Drops consecutive identical coordinates, returning null when fewer than two remain. Cutting a sub-line and then
    // rounding it to centimetres can make an interpolated cut point that lands a few millimetres past a vertex
    // coincide with that vertex; the resulting zero-length segment makes the linestring invalid for SQL Server
    // (error 24406, "curve (n) degenerates to a point").
    public static LineString? WithoutRepeatedCoordinates(this LineString lineString)
    {
        ArgumentNullException.ThrowIfNull(lineString);

        var coordinates = CoordinateArrays.RemoveRepeatedPoints(lineString.Coordinates);
        if (coordinates.Length < 2)
        {
            return null;
        }

        return coordinates.Length == lineString.NumPoints
            ? lineString
            : lineString.Factory.CreateLineString(coordinates).WithSrid(lineString.SRID);
    }

    public static T RoundToCm<T>(this T geometry)
        where T : Geometry
    {
        return geometry.RoundCoordinates(2);
    }

    // Whether a geometry carries more precision than the register works in. Everything stored goes through RoundToCm,
    // so a caller supplying more than that would silently get back something other than what they sent: the endpoints
    // refuse it rather than discard it.
    public static bool HasCoordinatesMorePreciseThanCm(this Geometry geometry)
    {
        return geometry.Coordinates.Any(c => IsMorePreciseThanCm(c.X) || IsMorePreciseThanCm(c.Y));
    }

    private static bool IsMorePreciseThanCm(double value)
    {
        // Math.Round returns the bit-identical double for a value that is already at two decimals, so an honest
        // coordinate compares exactly equal even at Lambert magnitudes.
        return Math.Abs(value - Math.Round(value, 2)) > 1e-10;
    }
    public static RoadNodeGeometry RoundToCm(this RoadNodeGeometry geometry)
    {
        return RoadNodeGeometry.Create(geometry.Value.RoundToCm());
    }
    public static RoadSegmentGeometry RoundToCm(this RoadSegmentGeometry geometry)
    {
        return RoadSegmentGeometry.Create(geometry.Value.RoundToCm());
    }
    public static JunctionGeometry RoundToCm(this JunctionGeometry geometry)
    {
        return JunctionGeometry.Create(geometry.Value.RoundToCm());
    }

    public static Coordinate RoundToCm(this Coordinate coordinate)
    {
        return new Coordinate(coordinate.X.RoundToCm(), coordinate.Y.RoundToCm());
    }

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

    public static JunctionGeometry EnsureLambert08(this JunctionGeometry geometry)
    {
        if (geometry.Value.IsLambert08())
        {
            return geometry;
        }

        if (geometry.Value.IsLambert72())
        {
            return JunctionGeometry.Create(geometry.Value.TransformFromLambert72To08());
        }

        throw new InvalidCastException($"Geometry SRID {geometry.SRID} is not Lambert72 or Lambert08");
    }
    public static JunctionGeometry EnsureLambert72(this JunctionGeometry geometry)
    {
        if (geometry.Value.IsLambert72())
        {
            return geometry;
        }

        if (geometry.Value.IsLambert08())
        {
            return JunctionGeometry.Create(geometry.Value.TransformFromLambert08To72());
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
