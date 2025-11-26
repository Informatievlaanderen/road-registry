namespace RoadRegistry.RoadSegment;

using System;
using BackOffice.Core;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadNetwork.ValueObjects;
using ValueObjects;
using LineString = NetTopologySuite.Geometries.LineString;

public static class NetTopologySuiteExtensions
{
    public static MultiLineString ToMultiLineString(this GeometryObject geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return ((MultiLineString)new WKTReader().Read(geometry.WKT)
            .WithSrid(geometry.SRID))
            .WithMeasureOrdinates();
    }

    public static GeometryObject ToGeometryObject(this MultiLineString geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return new GeometryObject(geometry.SRID, geometry
            .WithoutDuplicateCoordinates()
            .AsText());
    }

    public static Problems GetProblemsForRoadSegmentOutlinedGeometry(this LineString line, RoadSegmentId id)
    {
        var problems = Problems.None;
        var contextTolerances = VerificationContextTolerances.Default;

        if (line.Length.IsReasonablyLessThan(Distances.TooClose, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentGeometryLengthIsLessThanMinimum(id, Distances.TooClose));
        }

        if (!line.Length.IsReasonablyLessThan(Distances.TooLongSegmentLength, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentGeometryLengthIsTooLong(id, Distances.TooLongSegmentLength));
        }

        return problems;
    }

    public static Problems ValidateRoadSegmentGeometry(this LineString? line, RoadSegmentId id)
    {
        var problems = Problems.None;
        if (line is null)
        {
            return problems;
        }

        var contextTolerances = VerificationContextTolerances.Default;

        if (line.Length.IsReasonablyEqualTo(0, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentGeometryLengthIsZero(id));
        }

        if (!line.Length.IsReasonablyLessThan(Distances.TooLongSegmentLength, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentGeometryLengthIsTooLong(id, Distances.TooLongSegmentLength));
        }

        if (line.SelfOverlaps())
        {
            problems = problems.Add(new RoadSegmentGeometrySelfOverlaps(id));
        }
        else if (line.SelfIntersects())
        {
            problems = problems.Add(new RoadSegmentGeometrySelfIntersects(id));
        }

        if (line.NumPoints > 0)
        {
            var previousPointMeasure = 0.0;
            for (var index = 0; index < line.CoordinateSequence.Count; index++)
            {
                var measure = line.CoordinateSequence.GetOrdinate(index, Ordinate.M);
                var x = line.CoordinateSequence.GetX(index);
                var y = line.CoordinateSequence.GetY(index);
                if (index == 0 && !measure.IsReasonablyEqualTo(0, contextTolerances))
                {
                    problems =
                        problems.Add(new RoadSegmentStartPointMeasureValueNotEqualToZero(id, x, y, measure));
                }
                else if (index == line.CoordinateSequence.Count - 1 &&
                         !measure.IsReasonablyEqualTo(line.Length, contextTolerances))
                {
                    problems =
                        problems.Add(new RoadSegmentEndPointMeasureValueNotEqualToLength(id, x, y, measure, line.Length));
                }
                else if (measure < 0.0 || measure.IsReasonablyGreaterThan(line.Length, contextTolerances))
                {
                    problems =
                        problems.Add(new RoadSegmentPointMeasureValueOutOfRange(id, x, y, measure, 0.0, line.Length));
                }
                else
                {
                    if (index != 0 && measure <= previousPointMeasure)
                    {
                        problems =
                            problems.Add(new RoadSegmentPointMeasureValueDoesNotIncrease(id, x, y, measure,
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
