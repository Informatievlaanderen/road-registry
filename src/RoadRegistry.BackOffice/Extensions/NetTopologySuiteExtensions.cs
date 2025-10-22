namespace NetTopologySuite.Geometries;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using IO;
using Microsoft.Data.SqlClient;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using DynamicRoadSegmentAttribute = RoadRegistry.BackOffice.Core.DynamicRoadSegmentAttribute;

public static class NetTopologySuiteExtensions
{
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
            && !previousLane.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances))
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
            && !previousWidth.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances))
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
            && !previousSurface.To.ToDouble().IsReasonablyEqualTo(line.Length, contextTolerances))
        {
            problems = problems.Add(new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(
                previousSurface.TemporaryId, previousSurface.To, line.Length));
        }

        return problems;
    }

    public static SqlParameter ToSqlParameter(this Geometry geometry, string name)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var writer = new SqlServerBytesWriter { IsGeography = false };
        var bytes = writer.Write(geometry);
        return new SqlParameter($"@{name}", SqlDbType.Udt)
        {
            UdtTypeName = "geometry",
            SqlValue = new SqlBytes(bytes)
        };
    }
}
