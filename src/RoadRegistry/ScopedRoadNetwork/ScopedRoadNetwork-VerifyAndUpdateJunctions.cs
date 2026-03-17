namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Linq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    //TODO-pr add unit tests to manage gradejunctions
    private Problems VerifyAndUpdateJunctions(IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context, RoadNetworkChangesSummary summary)
    {
        var problems = Problems.None;

        // Get all "gerealiseerde" segments for comparison
        var allGerealiseerdeSegments = GetNonRemovedRoadSegments()
            .Where(x => x.Attributes.Status == RoadSegmentStatusV2.Gerealiseerd)
            .ToArray();

        // Get all "gerealiseerde" segments that have changes
        var changedGerealiseerdeSegments = allGerealiseerdeSegments
            .Where(x => x.HasChanges())
            .ToArray();

        if (!changedGerealiseerdeSegments.Any())
        {
            return problems;
        }

        // Find all unique combinations of changed segments with all gerealiseerde segments
        var segmentCombinationsOfChangedSegments = (
            from changed in changedGerealiseerdeSegments
            from other in allGerealiseerdeSegments
            where changed.RoadSegmentId != other.RoadSegmentId
                  && changed.Geometry.Value.Envelope.Intersects(other.Geometry.Value.Envelope)
            let intersectionPoints = GetIntersectionPoints(changed.Geometry, other.Geometry)
            select (
                Segment1: changed,
                Segment2: other,
                Key: CreateCombinationKey(changed.RoadSegmentId, other.RoadSegmentId),
                IntersectingPoints: intersectionPoints
            )
        ).DistinctBy(x => x.Key).ToArray();

        // Get existing junctions
        var existingGradeSeparatedJunctions = _gradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.LowerRoadSegmentId, x.UpperRoadSegmentId), x => x);
        var existingGradeJunctions = _gradeJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.RoadSegmentId1, x.RoadSegmentId2), x => x);

        // Remove grade junctions that are not intersecting any segment
        foreach (var combination in segmentCombinationsOfChangedSegments
                     .Where(x => x.IntersectingPoints.Length == 0))
        {
            if (existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
            {
                problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context, summary);
            }
        }

        // Validate for multiple intersections per segment combination
        foreach (var segmentCombination in segmentCombinationsOfChangedSegments
                     .Where(x => x.IntersectingPoints.Length > 1))
        {
            var problemContext = ProblemContext.For(context.IdTranslator.TranslateToTemporaryId(segmentCombination.Segment1.RoadSegmentId));
            problems += new RoadSegmentDuplicateIntersections(context.IdTranslator.TranslateToTemporaryId(segmentCombination.Segment2.RoadSegmentId))
                .WithContext(problemContext);
        }

        // Find actual intersections that are not at road nodes (start/end points)
        // Validate grade separated junctions and create missing grade junctions
        foreach (var combination in segmentCombinationsOfChangedSegments
                     .Where(x => x.IntersectingPoints.Length == 1))
        {
            var segment1Geometry = combination.Segment1.Geometry.Value.GetSingleLineString();
            var segment2Geometry = combination.Segment2.Geometry.Value.GetSingleLineString();
            var intersection = combination.IntersectingPoints.Single();

            // Skip intersections that are at start/end nodes (those are handled by road nodes)
            var isFarAwayFromNodes = intersection.IsFarEnoughAwayFrom(
                [
                    segment1Geometry.StartPoint, segment1Geometry.EndPoint,
                    segment2Geometry.StartPoint, segment2Geometry.EndPoint
                ],
                context.Tolerances.GeometryTolerance);

            if (!isFarAwayFromNodes)
            {
                continue;
            }

            var hasOverlapInTrafficTypes = combination.Segment1.HasTrafficOverlap(combination.Segment2, intersection, context);

            if (hasOverlapInTrafficTypes && !existingGradeSeparatedJunctions.ContainsKey(combination.Key))
            {
                var problemContext = ProblemContext.For(context.IdTranslator.TranslateToTemporaryId(combination.Segment1.RoadSegmentId));
                problems += new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(context.IdTranslator.TranslateToTemporaryId(combination.Segment2.RoadSegmentId))
                    .WithContext(problemContext);
            }

            if (!hasOverlapInTrafficTypes && !existingGradeJunctions.ContainsKey(combination.Key))
            {
                problems += AddGradeJunction(combination.Segment1.RoadSegmentId, combination.Segment2.RoadSegmentId, idGenerator, context, summary);
            }
        }

        return problems;
    }

    private static Point[] GetIntersectionPoints(RoadSegmentGeometry geometry1, RoadSegmentGeometry geometry2)
    {
        return geometry1.Value.Intersection(geometry2.Value).Coordinates
            .Select(c => geometry1.Value.Factory.CreatePoint(c))
            .ToArray();
    }

    private static (int, int) CreateCombinationKey(RoadSegmentId id1, RoadSegmentId id2)
    {
        var min = Math.Min(id1.ToInt32(), id2.ToInt32());
        var max = Math.Max(id1.ToInt32(), id2.ToInt32());
        return (min, max);
    }
}
