namespace NetTopologySuite.Geometries;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Polly;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;

public static class NetTopologySuiteExtensions
{
    public static IPolygonal[] GetPolygonals(this IPolygonal polygonal)
    {
        if (polygonal is Polygon polygon)
        {
            return new IPolygonal[] { polygon };
        }

        if (polygonal is MultiPolygon multiPolygon)
        {
            return multiPolygon.Geometries.Cast<IPolygonal>().ToArray();
        }

        throw new NotSupportedException($"Type '{polygonal.GetType().FullName}' is not supported. Only 'Polygon' or 'MultiPolygon' are allowed.");
    }

    public static Problems GetProblemsForRoadSegmentOutlinedGeometry(this LineString line, VerificationContextTolerances contextTolerances)
    {
        var problems = Problems.None;

        if (Math.Abs(line.Length) <= Distances.TooClose)
        {
            problems = problems.Add(new RoadSegmentGeometryLengthIsLessThanMinimum(Distances.TooClose));
        }

        return problems;
    }

    public static Problems GetProblemsForRoadSegmentGeometry(this LineString line, VerificationContextTolerances contextTolerances)
    {
        var problems = Problems.None;
        
        if (Math.Abs(line.Length) <= contextTolerances.GeometryTolerance)
        {
            problems = problems.Add(new RoadSegmentGeometryLengthIsZero());
        }
        
        if (line.SelfOverlaps())
        {
            problems = problems.Add(new RoadSegmentGeometrySelfOverlaps());
        }
        else if (line.SelfIntersects())
        {
            problems = problems.Add(new RoadSegmentGeometrySelfIntersects());
        }

        if (line.NumPoints > 0)
        {
            var previousPointMeasure = 0.0;
            for (var index = 0; index < line.CoordinateSequence.Count; index++)
            {
                var measure = line.CoordinateSequence.GetOrdinate(index, Ordinate.M);
                var x = line.CoordinateSequence.GetX(index);
                var y = line.CoordinateSequence.GetY(index);
                if (index == 0 && Math.Abs(measure) > contextTolerances.MeasurementTolerance)
                {
                    problems =
                        problems.Add(new RoadSegmentStartPointMeasureValueNotEqualToZero(x, y, measure));
                }
                else if (index == line.CoordinateSequence.Count - 1 &&
                         Math.Abs(measure - line.Length) > contextTolerances.MeasurementTolerance)
                {
                    problems =
                        problems.Add(new RoadSegmentEndPointMeasureValueNotEqualToLength(x, y, measure, line.Length));
                }
                else if (measure < 0.0 || measure - line.Length > contextTolerances.MeasurementTolerance)
                {
                    problems =
                        problems.Add(new RoadSegmentPointMeasureValueOutOfRange(x, y, measure, 0.0, line.Length));
                }
                else
                {
                    if (index != 0 && Math.Sign(measure - previousPointMeasure) <= 0)
                    {
                        problems =
                            problems.Add(new RoadSegmentPointMeasureValueDoesNotIncrease(x, y, measure,
                                previousPointMeasure));
                    }
                    else
                    {
                        previousPointMeasure = measure;
                    }
                }
            }
        }

        return problems;
    }

    public static Problems GetProblemsForRoadSegmentLanes<T>(this LineString line, IEnumerable<T> lanes, VerificationContextTolerances contextTolerances)
        where T: DynamicRoadSegmentAttribute
    {
        var problems = Problems.None;

        T previousLane = null;
        foreach (var lane in lanes)
        {
            if (previousLane == null)
            {
                if (lane.From != RoadSegmentPosition.Zero)
                {
                    problems =
                        problems.Add(new RoadSegmentLaneAttributeFromPositionNotEqualToZero(
                            lane.TemporaryId,
                            lane.From));
                }
            }
            else
            {
                if (lane.From != previousLane.To)
                {
                    problems =
                        problems.Add(new RoadSegmentLaneAttributesNotAdjacent(
                            previousLane.TemporaryId,
                            previousLane.To,
                            lane.TemporaryId,
                            lane.From));
                }

                if (lane.From == lane.To)
                {
                    problems =
                        problems.Add(new RoadSegmentLaneAttributeHasLengthOfZero(
                            lane.TemporaryId,
                            lane.From,
                            lane.To));
                }
            }

            previousLane = lane;
        }

        if (previousLane != null
            && !previousLane.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances.DynamicRoadSegmentAttributePositionTolerance))
        {
            problems = problems.Add(new RoadSegmentLaneAttributeToPositionNotEqualToLength(
                previousLane.TemporaryId,
                previousLane.To,
                line.Length));
        }

        return problems;
    }

    public static Problems GetProblemsForRoadSegmentWidths<T>(this LineString line, IEnumerable<T> widths, VerificationContextTolerances contextTolerances)
        where T : DynamicRoadSegmentAttribute
    {
        var problems = Problems.None;

        T previousWidth = null;
        foreach (var width in widths)
        {
            if (previousWidth == null)
            {
                if (width.From != RoadSegmentPosition.Zero)
                {
                    problems =
                        problems.Add(new RoadSegmentWidthAttributeFromPositionNotEqualToZero(
                            width.TemporaryId,
                            width.From));
                }
            }
            else
            {
                if (width.From != previousWidth.To)
                {
                    problems =
                        problems.Add(new RoadSegmentWidthAttributesNotAdjacent(
                            previousWidth.TemporaryId,
                            previousWidth.To,
                            width.TemporaryId,
                            width.From));
                }

                if (width.From == width.To)
                {
                    problems =
                        problems.Add(new RoadSegmentWidthAttributeHasLengthOfZero(
                            width.TemporaryId,
                            width.From,
                            width.To));
                }
            }

            previousWidth = width;
        }

        if (previousWidth != null
            && !previousWidth.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances.DynamicRoadSegmentAttributePositionTolerance))
        {
            problems = problems.Add(new RoadSegmentWidthAttributeToPositionNotEqualToLength(
                previousWidth.TemporaryId,
                previousWidth.To,
                line.Length));
        }

        return problems;
    }

    public static Problems GetProblemsForRoadSegmentSurfaces<T>(this LineString line, IEnumerable<T> surfaces, VerificationContextTolerances contextTolerances)
        where T : DynamicRoadSegmentAttribute
    {
        var problems = Problems.None;

        T previousSurface = null;
        foreach (var surface in surfaces)
        {
            if (previousSurface == null)
            {
                if (surface.From != RoadSegmentPosition.Zero)
                {
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(
                            surface.TemporaryId,
                            surface.From));
                }
            }
            else
            {
                if (surface.From != previousSurface.To)
                {
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributesNotAdjacent(
                            previousSurface.TemporaryId,
                            previousSurface.To,
                            surface.TemporaryId,
                            surface.From));
                }

                if (surface.From == surface.To)
                {
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributeHasLengthOfZero(
                            surface.TemporaryId,
                            surface.From,
                            surface.To));
                }
            }

            previousSurface = surface;
        }

        if (previousSurface != null
            && !previousSurface.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances.DynamicRoadSegmentAttributePositionTolerance))
        {
            problems = problems.Add(new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(
                previousSurface.TemporaryId, previousSurface.To, line.Length));
        }

        return problems;
    }

    public static bool RoadSegmentOverlapsWith(this MultiLineString g0, MultiLineString g1)
    {
        var openGisGeometryType = OgcGeometryType.LineString;
        var criticalOverlapPercentage = 0.7;
        var bufferSize = DefaultTolerances.IntersectionBuffer;

        return OverlapsWith(g0, g1, criticalOverlapPercentage, openGisGeometryType, bufferSize);
    }
    public static bool OverlapsWith(this Geometry g0, Geometry g1, double threshold, OgcGeometryType oGisGeometryType, double clusterTolerance)
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

    public static bool IsReasonablyEqualTo(this Geometry g0, Geometry g1, double clusterTolerance)
    {
        //Catch weird comparisons...
        if (g0 is null && g1 is null)
        {
            return true;
        }

        if (g0 is null || g1 is null)
        {
            return false;
        }

        // check if geometries are reasonably equal by checking if they fit within eachother's buffer
        var g0Buf = g0.Buffer(clusterTolerance);
        var g1Buf = g1.Buffer(clusterTolerance);

        if (!g0.Within(g1Buf))
            return false;
        if (!g1.Within(g0Buf))
            return false;

        return true;
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
            return new MultiPolygon(new[] { polygon }, polygon.Factory)
            {
                SRID = polygon.SRID
            };
        }

        throw new InvalidCastException($"The geometry of type {geometry.GetType().Name} must be either a {nameof(Polygon)} or a {nameof(MultiPolygon)}.");
    }

    public static LineString GetSingleLineString(this MultiLineString geometry)
    {
        return geometry.Geometries
            .OfType<LineString>()
            .Single();
    }

    public static MultiLineString ToMultiLineString(this Geometry geometry, GeometryFactory geometryFactory = null)
    {
        if (geometry is MultiLineString multiLineString)
        {
            return multiLineString;
        }

        if (geometry is LineString lineString)
        {
            return new MultiLineString(new[] { lineString }, geometryFactory ?? lineString.Factory)
            {
                SRID = lineString.SRID
            };
        }

        throw new InvalidCastException($"The geometry of type {geometry.GetType().Name} must be either a {nameof(LineString)} or a {nameof(MultiLineString)}.");
    }

    public static MultiPoint ToMultiPoint(this Geometry geometry, GeometryFactory geometryFactory = null)
    {
        if (geometry is MultiPoint multiPoint)
        {
            return multiPoint;
        }

        if (geometry is Point point)
        {
            return new MultiPoint(new[] { point }, geometryFactory ?? point.Factory)
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
}
