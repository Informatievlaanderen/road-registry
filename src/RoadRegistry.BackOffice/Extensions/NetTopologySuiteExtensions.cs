namespace NetTopologySuite.Geometries;

using System;
using System.Linq;
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

    public static bool OverlapsWith(this Geometry g0, Geometry g1, double threshold, OgcGeometryType oGisGeometryType, double clusterTolerance)
    {
        if (g0 == null && g1 == null)
        {
            return true;
        }

        if ((g0 == null && g1 != null) || (g0 != null && g1 == null))
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
            var overlapValue = Math.Round((double)((overlap.Length * 100) / g1.Length));
            if (overlapValue >= threshold)
            {
                return CheckOverlapViceVersa(g0, g1, OgcGeometryType.LineString, threshold, clusterTolerance);
            }

            return false;
        }

        if (oGisGeometryType == OgcGeometryType.Polygon)
        {
            overlap = g0.Intersection(g1);

            var overlapValue = Math.Round((double)((overlap.Area * 100) / g1.Area));
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
        if (g0 == null && g1 == null)
        {
            return true;
        }
        if ((g0 == null && g1 != null) || (g0 != null && g1 == null))
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
            var overlapValue = Math.Round((double)((overlap.Length * 100) / g0.Length));
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
            var overlapValue = Math.Round((double)((overlap.Area * 100) / g0.Area));
            if (overlapValue >= threshold)
            {
                return true;
            }

            return false;
        }
    }
}
