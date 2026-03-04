namespace RoadRegistry.RoadSegment;

using System.Linq;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.ValueObjects.Problems;
using LineString = NetTopologySuite.Geometries.LineString;

public static class NetTopologySuiteExtensions
{
    public static Problems ValidateRoadSegmentGeometryDomainV2(this RoadSegmentGeometry? geometry, RoadSegmentId id)
    {
        if (geometry is null)
        {
            return Problems.None;
        }

        return ValidateRoadSegmentGeometryDomainV2(geometry.Value, id);
    }

    public static Problems ValidateRoadSegmentGeometryDomainV2(this MultiLineString? multiLineString, RoadSegmentId id)
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
            return Problems.Single(new RoadSegmentGeometryLineCountMismatch(id, 1, lines.Length));
        }

        return ValidateRoadSegmentGeometryDomainV2(multiLineString.GetSingleLineString(), id);
    }

    private static Problems ValidateRoadSegmentGeometryDomainV2(this LineString? line, RoadSegmentId id)
    {
        if (line is null)
        {
            return Problems.None;
        }

        line = line.WithoutDuplicateCoordinates();

        var problems = Problems.None;
        var tolerances = VerificationContextTolerances.Cm;

        if (line.Length < 1)
        {
            problems += new RoadSegmentGeometryLengthIsLessThanMinimum(id, 1);
        }

        if (!line.Length.IsReasonablyLessThan(Distances.TooLongSegmentLength, tolerances))
        {
            problems += new RoadSegmentGeometryLengthIsTooLong(id, Distances.TooLongSegmentLength);
        }

        if (line.ContainsVertexTooCloseToAnother(0.15))
        {
            problems += new RoadSegmentGeometryVerticesTooClose(id);
        }

        if (line.SelfOverlaps())
        {
            problems += new RoadSegmentGeometrySelfOverlaps(id);
        }
        else if (line.SelfIntersects())
        {
            problems += new RoadSegmentGeometrySelfIntersects(id);
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
