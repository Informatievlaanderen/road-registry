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
}
