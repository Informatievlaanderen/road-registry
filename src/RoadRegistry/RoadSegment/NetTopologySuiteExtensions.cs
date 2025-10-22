namespace RoadRegistry.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadNetwork.ValueObjects;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.RoadSegment.ValueObjects;
using LineString = NetTopologySuite.Geometries.LineString;

public static class NetTopologySuiteExtensions
{
    public static MultiLineString ToMultiLineString(this RoadSegmentGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var toLineStrings = new List<LineString>();
        foreach (var fromLineString in geometry.MultiLineString)
        {
            var toPoints = new List<Coordinate>();
            for (var index = 0; index < fromLineString.Points.Length && index < fromLineString.Measures.Length; index++)
            {
                var fromPoint = fromLineString.Points[index];
                var fromMeasure = fromLineString.Measures[index];
                toPoints.Add(new CoordinateM(fromPoint.X, fromPoint.Y, fromMeasure));
            }

            toLineStrings.Add(
                new LineString(
                        new CoordinateArraySequence(toPoints.ToArray()),
                        WellKnownGeometryFactories.Default)
                    .WithSrid(geometry.SpatialReferenceSystemIdentifier)
            );
        }

        return new MultiLineString(toLineStrings.ToArray(), WellKnownGeometryFactories.Default)
            .WithSrid(geometry.SpatialReferenceSystemIdentifier)
            .WithMeasureOrdinates();
    }

    public static RoadSegmentGeometry ToRoadSegmentGeometry(this MultiLineString geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        geometry = geometry
            .WithoutDuplicateCoordinates()
            .WithMeasureOrdinates();

        var toMultiLineString = new RoadRegistry.BackOffice.Messages.LineString[geometry.NumGeometries];
        var lineIndex = 0;
        foreach (var fromLineString in geometry.Geometries.OfType<LineString>())
        {
            var toLineString = new RoadRegistry.BackOffice.Messages.LineString
            {
                Points = new RoadRegistry.BackOffice.Messages.Point[fromLineString.NumPoints],
                Measures = fromLineString.GetOrdinates(Ordinate.M)
            };

            for (var pointIndex = 0; pointIndex < fromLineString.NumPoints; pointIndex++)
                toLineString.Points[pointIndex] = new RoadRegistry.BackOffice.Messages.Point
                {
                    X = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, Ordinate.X),
                    Y = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, Ordinate.Y)
                };

            toMultiLineString[lineIndex] = toLineString;
            lineIndex++;
        }

        return new RoadSegmentGeometry
        {
            SpatialReferenceSystemIdentifier = geometry.SRID,
            MultiLineString = toMultiLineString
        };
    }

    public static Problems GetProblemsForRoadSegmentOutlinedGeometry(this LineString line, RoadSegmentId id, VerificationContextTolerances contextTolerances)
    {
        var problems = Problems.None;

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

    public static Problems GetProblemsForRoadSegmentGeometry(this LineString line, RoadSegmentId id, VerificationContextTolerances contextTolerances)
    {
        var problems = Problems.None;

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

    public static Problems GetProblemsForRoadSegmentLanes<T>(this LineString line, IEnumerable<T> lanes, VerificationContextTolerances contextTolerances)
        where T: RoadSegmentDynamicAttributeChange
    {
        var problems = Problems.None;

        T? previousLane = null;
        foreach (var lane in lanes)
        {
            if (previousLane == null)
            {
                if (lane.From != RoadSegmentPosition.Zero)
                {
                    problems =
                        problems.Add(new RoadSegmentLaneAttributeFromPositionNotEqualToZero(
                            lane.Id,
                            lane.From));
                }
            }
            else
            {
                if (lane.From != previousLane.To)
                {
                    problems =
                        problems.Add(new RoadSegmentLaneAttributesNotAdjacent(
                            previousLane.Id,
                            previousLane.To,
                            lane.Id,
                            lane.From));
                }

                if (lane.From == lane.To)
                {
                    problems =
                        problems.Add(new RoadSegmentLaneAttributeHasLengthOfZero(
                            lane.Id,
                            lane.From,
                            lane.To));
                }
            }

            previousLane = lane;
        }

        if (previousLane != null
            && !previousLane.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentLaneAttributeToPositionNotEqualToLength(
                previousLane.Id,
                previousLane.To,
                line.Length));
        }

        return problems;
    }

    public static Problems GetProblemsForRoadSegmentWidths<T>(this LineString line, IEnumerable<T> widths, VerificationContextTolerances contextTolerances)
        where T : RoadSegmentDynamicAttributeChange
    {
        var problems = Problems.None;

        T? previousWidth = null;
        foreach (var width in widths)
        {
            if (previousWidth == null)
            {
                if (width.From != RoadSegmentPosition.Zero)
                {
                    problems =
                        problems.Add(new RoadSegmentWidthAttributeFromPositionNotEqualToZero(
                            width.Id,
                            width.From));
                }
            }
            else
            {
                if (width.From != previousWidth.To)
                {
                    problems =
                        problems.Add(new RoadSegmentWidthAttributesNotAdjacent(
                            previousWidth.Id,
                            previousWidth.To,
                            width.Id,
                            width.From));
                }

                if (width.From == width.To)
                {
                    problems =
                        problems.Add(new RoadSegmentWidthAttributeHasLengthOfZero(
                            width.Id,
                            width.From,
                            width.To));
                }
            }

            previousWidth = width;
        }

        if (previousWidth != null
            && !previousWidth.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentWidthAttributeToPositionNotEqualToLength(
                previousWidth.Id,
                previousWidth.To,
                line.Length));
        }

        return problems;
    }

    public static Problems GetProblemsForRoadSegmentSurfaces<T>(this LineString line, IEnumerable<T> surfaces, VerificationContextTolerances contextTolerances)
        where T : RoadSegmentDynamicAttributeChange
    {
        var problems = Problems.None;

        T? previousSurface = null;
        foreach (var surface in surfaces)
        {
            if (previousSurface == null)
            {
                if (surface.From != RoadSegmentPosition.Zero)
                {
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(
                            surface.Id,
                            surface.From));
                }
            }
            else
            {
                if (surface.From != previousSurface.To)
                {
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributesNotAdjacent(
                            previousSurface.Id,
                            previousSurface.To,
                            surface.Id,
                            surface.From));
                }

                if (surface.From == surface.To)
                {
                    problems =
                        problems.Add(new RoadSegmentSurfaceAttributeHasLengthOfZero(
                            surface.Id,
                            surface.From,
                            surface.To));
                }
            }

            previousSurface = surface;
        }

        if (previousSurface != null
            && !previousSurface.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(
                previousSurface.Id, previousSurface.To, line.Length));
        }

        return problems;
    }
}
