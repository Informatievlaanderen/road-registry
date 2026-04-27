namespace RoadRegistry.RoadSegment;

using System;
using System.Linq;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.ValueObjects.Problems;
using LineString = NetTopologySuite.Geometries.LineString;

public static class NetTopologySuiteExtensions
{
    public static Problems ValidateRoadSegmentGeometryDomainV2(this RoadSegmentGeometry? geometry)
    {
        if (geometry is null)
        {
            return Problems.None;
        }

        return ValidateRoadSegmentGeometryDomainV2(geometry.Value);
    }

    public static Problems ValidateRoadSegmentGeometryDomainV2(this MultiLineString? multiLineString, bool skipMinimumLengthCheck = false)
    {
        if (multiLineString is null)
        {
            return Problems.None;
        }

        var lines = multiLineString
            .Geometries
            .OfType<LineString>()
            .ToArray();
        if (lines.Length != 1)
        {
            return Problems.Single(new RoadSegmentGeometryLineCountMismatch(1, lines.Length));
        }

        return ValidateRoadSegmentGeometryDomainV2(multiLineString.GetSingleLineString(), skipMinimumLengthCheck);
    }

    public static Error? ValidateRoadSegmentGeometryMinimumLength(this LineString line)
    {
        if (line.Length < 1)
        {
            return new RoadSegmentGeometryLengthIsLessThanMinimum(1);
        }

        return null;
    }

    private static Problems ValidateRoadSegmentGeometryDomainV2(this LineString? line, bool skipMinimumLengthCheck)
    {
        if (line is null)
        {
            return Problems.None;
        }

        line = line.WithoutDuplicateCoordinates();

        var problems = Problems.None;
        var tolerances = VerificationContextTolerances.Cm;

        if (!skipMinimumLengthCheck)
        {
            var error = ValidateRoadSegmentGeometryMinimumLength(line);
            if (error is not null)
            {
                problems += error;
            }
        }

        if (!line.Length.IsReasonablyLessThan(Distances.TooLongSegmentLength, tolerances))
        {
            problems += new RoadSegmentGeometryLengthIsTooLong(Distances.TooLongSegmentLength);
        }

        if (line.StartPoint.IsReasonablyEqualTo(line.EndPoint, tolerances))
        {
            problems += new RoadSegmentGeometryStartEqualsEnd();
        }

        if (line.ContainsVertexTooCloseToAnother(0.15))
        {
            problems += new RoadSegmentGeometryVerticesTooClose();
        }

        if (line.SelfOverlaps())
        {
            problems += new RoadSegmentGeometrySelfOverlaps();
        }
        else if (line.SelfIntersects())
        {
            problems += new RoadSegmentGeometrySelfIntersects();
        }

        return problems;
    }

    private static bool ContainsVertexTooCloseToAnother(this LineString lineString, double minimumDistanceBetweenVertices)
    {
        var previousVertex = lineString.Coordinate;
        for (var i = 1; i < lineString.Count; i++)
        {
            var currentVertex = lineString.GetCoordinateN(i);
            if (currentVertex.Distance(previousVertex) < minimumDistanceBetweenVertices)
            {
                return true;
            }

            previousVertex = currentVertex;
        }

        return false;
    }
}
