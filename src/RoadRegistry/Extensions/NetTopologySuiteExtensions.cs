namespace RoadRegistry.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;

public static class NetTopologySuiteExtensions
{
    public static Point ToGeometry(this RoadNodeGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return ((Point)new WKTReader().Read(geometry.WKT)
            .WithSrid(geometry.SRID));
    }

    public static RoadNodeGeometry ToRoadNodeGeometry(this Point geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return RoadNodeGeometry.Create(geometry);
    }

    public static MultiLineString ToGeometry(this RoadSegmentGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return ((MultiLineString)new WKTReader().Read(geometry.WKT)
                .WithSrid(geometry.SRID))
            .WithMeasureOrdinates();
    }

    public static RoadSegmentGeometry ToRoadSegmentGeometry(this MultiLineString geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return RoadSegmentGeometry.Create(geometry.WithoutDuplicateCoordinates());
    }

    public static MultiPolygon ToGeometry(this ExtractGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return (MultiPolygon)new WKTReader().Read(geometry.WKT)
                .WithSrid(geometry.SRID);
    }

    public static ExtractGeometry ToExtractGeometry(this MultiPolygon geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return ExtractGeometry.Create(geometry);
    }

    public static IPolygonal[] GetPolygonals(this IPolygonal polygonal)
    {
        return polygonal switch
        {
            Polygon polygon => [polygon],
            MultiPolygon multiPolygon => multiPolygon.Geometries.Cast<IPolygonal>().ToArray(),
            _ => throw new NotSupportedException($"Type '{polygonal.GetType().FullName}' is not supported. Only 'Polygon' or 'MultiPolygon' are allowed.")
        };
    }

    public static bool RoadSegmentOverlapsWith(this MultiLineString g0, MultiLineString g1, double clusterTolerance)
    {
        var openGisGeometryType = OgcGeometryType.LineString;
        var criticalOverlapPercentage = 0.7;

        return OverlapsWith(g0, g1, criticalOverlapPercentage, openGisGeometryType, clusterTolerance);
    }

    private static bool OverlapsWith(Geometry? g0, Geometry? g1, double threshold, OgcGeometryType oGisGeometryType, double clusterTolerance)
    {
        if (g0 is null && g1 is null)
        {
            return true;
        }

        if (g0 is null || g1 is null)
        {
            return false;
        }

        Geometry overlap;

        if (oGisGeometryType == OgcGeometryType.Point)
        {
            var g0Buf = g0.Buffer(clusterTolerance);
            var g1Buf = g1.Buffer(clusterTolerance);
            if (!g0.Within(g1Buf))
                return false;
            if (!g1.Within(g0Buf))
                return false;
            return true;
        }

        if (oGisGeometryType == OgcGeometryType.LineString)
        {
            if (g1.Length < 1.42)
            {
                clusterTolerance = g1.Length / 2;
            }
            var g1Buf = g1.Buffer(clusterTolerance);
            overlap = g0.Intersection(g1Buf);
            var overlapValue = Math.Round(overlap.Length / g1.Length);
            if (overlapValue >= threshold)
            {
                return CheckOverlapViceVersa(g0, g1, OgcGeometryType.LineString, threshold, clusterTolerance);
            }

            return false;
        }

        if (oGisGeometryType == OgcGeometryType.Polygon)
        {
            overlap = g0.Intersection(g1);

            var overlapValue = Math.Round(overlap.Area / g1.Area);
            if (overlapValue >= threshold)
            {
                return true;
            }

            return false;
        }

        throw new NotSupportedException($"{nameof(OgcGeometryType)}.{oGisGeometryType} is not supported");
    }

    private static bool CheckOverlapViceVersa(Geometry g0, Geometry g1, OgcGeometryType oGisGeometryType, double threshold, double compareTolerance)
    {
        if (oGisGeometryType == OgcGeometryType.LineString)
        {
            var g0Buf = g0.Buffer(compareTolerance);
            var overlap = g1.Intersection(g0Buf);
            var overlapValue = Math.Round(overlap.Length / g0.Length);
            if (overlapValue >= threshold)
            {
                return true;
            }

            return false;
        }
        else
        {
            //omgekeerde moet ook gecheckt worden (voorkomen vergelijking met verkeerd omvattend feature, overlap = 100%)
            var overlap = g1.Intersection(g0);
            var overlapValue = Math.Round(overlap.Area / g0.Area);
            if (overlapValue >= threshold)
            {
                return true;
            }

            return false;
        }
    }

    public static bool IsFarEnoughAwayFrom(this Point intersection, IEnumerable<Point> points, double tolerance)
    {
        return points.All(point => !intersection.IsWithinDistance(point, tolerance));
    }

    public static MultiPolygon ToMultiPolygon(this Geometry geometry)
    {
        if (geometry is MultiPolygon multiPolygon)
        {
            return multiPolygon;
        }

        if (geometry is Polygon polygon)
        {
            return new MultiPolygon([polygon], polygon.Factory)
            {
                SRID = polygon.SRID
            };
        }

        throw new InvalidCastException($"The geometry of type {geometry.GetType().Name} must be either a {nameof(Polygon)} or a {nameof(MultiPolygon)}.");
    }

    public static bool HasExactlyOneLineString(this MultiLineString geometry)
    {
        return geometry.Geometries
            .OfType<LineString>()
            .Count() == 1;
    }

    public static LineString GetSingleLineString(this MultiLineString geometry)
    {
        return geometry.Geometries
            .OfType<LineString>()
            .Single();
    }

    public static MultiLineString ToMultiLineString(this Geometry geometry, GeometryFactory? geometryFactory = null)
    {
        if (geometry is MultiLineString multiLineString)
        {
            return multiLineString;
        }

        if (geometry is LineString lineString)
        {
            return new MultiLineString([lineString], geometryFactory ?? lineString.Factory)
                .WithSrid(lineString.SRID);
        }

        throw new InvalidCastException($"The geometry of type {geometry.GetType().Name} must be either a {nameof(LineString)} or a {nameof(MultiLineString)}.");
    }

    public static MultiPoint ToMultiPoint(this Geometry geometry, GeometryFactory? geometryFactory = null)
    {
        if (geometry is MultiPoint multiPoint)
        {
            return multiPoint;
        }

        if (geometry is Point point)
        {
            return new MultiPoint([point], geometryFactory ?? point.Factory)
            {
                SRID = point.SRID
            };
        }

        throw new InvalidCastException($"The geometry of type {geometry.GetType().Name} must be either a {nameof(Point)} or a {nameof(MultiPoint)}.");
    }

    public static T WithSrid<T>(this T geometry, int srid)
        where T : Geometry
    {
        geometry.SRID = srid > 0
            ? srid
            : GeometryConfiguration.GeometryFactory.SRID;

        return geometry;
    }

    public static MultiLineString WithMeasureOrdinates(this MultiLineString multiLineString)
    {
        var lineStrings = multiLineString.Geometries
            .Cast<LineString>()
            .Select(lineString =>
            {
                if (lineString.Count == 0)
                {
                    return lineString;
                }

                var coordinates = new Coordinate[lineString.Count];
                coordinates[0] = new CoordinateM(lineString.StartPoint.X, lineString.StartPoint.Y, 0);

                var currentMeasure = coordinates[0].M;

                for (var i = 1; i < lineString.Count; i++)
                {
                    var currentPoint = lineString[i];
                    var previousPoint = lineString[i - 1];

                    var distanceToPreviousPoint = previousPoint.Distance(currentPoint);
                    currentMeasure += distanceToPreviousPoint;
                    coordinates[i] = new CoordinateM(currentPoint.X, currentPoint.Y, currentMeasure);
                }

                return new LineString(new CoordinateArraySequence(coordinates), multiLineString.Factory)
                    .WithSrid(multiLineString.SRID);
            })
            .ToArray();

        return new MultiLineString(lineStrings, multiLineString.Factory)
            .WithSrid(multiLineString.SRID);
    }

    public static MultiLineString WithoutDuplicateCoordinates(this MultiLineString multiLineString)
    {
        var lineStrings = multiLineString.Geometries
            .Cast<LineString>()
            .Select(lineString =>
            {
                if (lineString.Count == 0)
                {
                    return lineString;
                }

                var coordinates = new CoordinateList
                {
                    new Coordinate(lineString.StartPoint.X, lineString.StartPoint.Y)
                };

                for (var i = 1; i < lineString.Count; i++)
                {
                    var currentPoint = lineString[i];
                    var previousPoint = lineString[i - 1];

                    if (currentPoint.Equals2D(previousPoint, DefaultTolerances.DuplicateCoordinatesTolerance))
                    {
                        if (i == lineString.Count - 1)
                        {
                            coordinates.RemoveAt(coordinates.Count - 1);
                            coordinates.Add(currentPoint);
                        }
                        continue;
                    }

                    coordinates.Add(currentPoint);
                }

                return multiLineString.Factory.CreateLineString(coordinates.ToCoordinateArray())
                    .WithSrid(multiLineString.SRID);
            })
            .ToArray();

        return new MultiLineString(lineStrings, multiLineString.Factory)
            .WithSrid(multiLineString.SRID);
    }

    public static bool SelfIntersects(this LineString instance)
    {
        if (instance.Length <= 0.0 || instance.NumPoints <= 2)
            return false;

        return !instance.IsSimple;
    }

    public static bool SelfOverlaps(this LineString instance)
    {
        if (instance.Length <= 0.0 || instance.NumPoints <= 2)
            return false;

        var lines = new LineString[instance.NumPoints - 1];
        var fromPoint = instance.StartPoint;
        for (var index = 1; index < instance.NumPoints; index++)
        {
            var toPoint = instance.GetPointN(index);
            lines[index - 1] =
                new LineString(
                    new CoordinateArraySequence(
                        new[]
                        {
                            new Coordinate(Math.Round(fromPoint.X, Precisions.GeometryPrecision), Math.Round(fromPoint.Y, Precisions.GeometryPrecision)),
                            new Coordinate(Math.Round(toPoint.X, Precisions.GeometryPrecision), Math.Round(toPoint.Y, Precisions.GeometryPrecision))
                        })
                    , WellKnownGeometryFactories.Default);
            fromPoint = toPoint;
        }

        var overlappings =
            (
                from left in lines
                from right in lines
                where !ReferenceEquals(left, right)
                select new
                {
                    Left = left,
                    Right = right,
                    LeftOverlapsRight = left.Overlaps(right),
                    LeftCoversRight = left.Covers(right)
                }
            )
            .Where(x => x.LeftOverlapsRight || x.LeftCoversRight)
            .ToArray();

        return overlappings.Any();
    }

    public static bool HasInvalidMeasureOrdinates(this LineString instance)
    {
        var measures = instance.GetOrdinates(Ordinate.M);
        return measures.Any(value => double.IsNaN(value) || double.IsNegativeInfinity(value) || double.IsPositiveInfinity(value));
    }

    public static bool IsReasonablyEqualTo(this MultiLineString @this, MultiLineString other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumGeometries != other.NumGeometries) return false;
        for (var i = 0; i < @this.NumGeometries; i++)
        {
            var thisLineString = (LineString)@this.GetGeometryN(i);
            var otherLineString = (LineString)other.GetGeometryN(i);
            if (!thisLineString.IsReasonablyEqualTo(otherLineString, tolerances)) return false;
        }

        return true;
    }

    private static bool IsReasonablyEqualTo(this LineString @this, LineString other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumPoints != other.NumPoints) return false;
        for (var i = 0; i < @this.NumPoints; i++)
        {
            var thisPoint = @this.GetCoordinateN(i);
            var otherPoint = other.GetCoordinateN(i);
            if (!thisPoint.IsReasonablyEqualTo(otherPoint, tolerances)) return false;
        }

        return true;
    }

    public static bool IsReasonablyEqualTo(this Point @this, Point other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.IsEmpty && other.IsEmpty) return true;
        if (@this.IsEmpty != other.IsEmpty) return false;
        return @this.EqualsExact(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyEqualTo(this Coordinate @this, Coordinate other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        return @this.Equals2D(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyEqualTo(this double value, double other)
    {
        return value.IsReasonablyEqualTo(other, VerificationContextTolerances.Default);
    }
    public static bool IsReasonablyEqualTo(this double value, double other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyEqualTo(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyEqualTo(this decimal value, decimal other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyEqualTo(other, (decimal)tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyLessThan(this double value, double other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyLessThan(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyLessThan(this decimal value, decimal other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyLessThan(other, (decimal)tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyGreaterThan(this double value, double other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyGreaterThan(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyGreaterThan(this decimal value, decimal other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyGreaterThan(other, (decimal)tolerances.GeometryTolerance);
    }
}
