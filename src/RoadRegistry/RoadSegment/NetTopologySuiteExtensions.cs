namespace RoadRegistry.RoadSegment;

using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.ValueObjects.Problems;
using LineString = NetTopologySuite.Geometries.LineString;

public static class NetTopologySuiteExtensions
{
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

        return problems;
    }
}
