namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Linq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
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
        var segmentCombinations = (
            from changed in changedGerealiseerdeSegments
            from other in allGerealiseerdeSegments
            where changed.RoadSegmentId != other.RoadSegmentId
                  && changed.Geometry.Value.Envelope.Intersects(other.Geometry.Value.Envelope)
            let intersectionCoordinates = changed.Geometry.Value.Intersection(other.Geometry.Value).Coordinates
            where intersectionCoordinates.Length > 0
            select (
                Segment1: changed,
                Segment2: other,
                Key: CreateCombinationKey(changed.RoadSegmentId, other.RoadSegmentId),
                IntersectingCoordinates: intersectionCoordinates
            )
        ).DistinctBy(x => x.Key).ToArray();

        // Validate for multiple intersections per segment combination
        var segmentCombinationsWithMultipleIntersections = segmentCombinations
            .Where(x => x.IntersectingCoordinates.Length > 1)
            .ToArray();
        foreach (var segmentCombination in segmentCombinationsWithMultipleIntersections)
        {
            var problemContext = ProblemContext.For(context.IdTranslator.TranslateToTemporaryId(segmentCombination.Segment1.RoadSegmentId));
            problems += new RoadSegmentDuplicateIntersections(context.IdTranslator.TranslateToTemporaryId(segmentCombination.Segment2.RoadSegmentId))
                .WithContext(problemContext);
        }

        // Get existing junctions
        var existingGradeSeparatedJunctions = _gradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.LowerRoadSegmentId, x.UpperRoadSegmentId), x => x);
        var existingGradeJunctions = _gradeJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.RoadSegmentId1, x.RoadSegmentId2), x => x);

        // Find actual intersections that are not at road nodes (start/end points)
        var validSegmentCombinations = segmentCombinations
            .Where(x => x.IntersectingCoordinates.Length == 1)
            .Select(combination =>
            {
                var segment1Geometry = combination.Segment1.Geometry.Value.GetSingleLineString();
                var segment2Geometry = combination.Segment2.Geometry.Value.GetSingleLineString();
                var intersection = combination.Segment1.Geometry.Value.Factory.CreatePoint(combination.IntersectingCoordinates.Single());

                // Skip intersections that are at start/end nodes (those are handled by road nodes)
                var isFarAwayFromNodes = intersection.IsFarEnoughAwayFrom(
                    [
                        segment1Geometry.StartPoint, segment1Geometry.EndPoint,
                        segment2Geometry.StartPoint, segment2Geometry.EndPoint
                    ],
                    context.Tolerances.GeometryTolerance);

                if (!isFarAwayFromNodes)
                {
                    return default;
                }

                var hasOverlapInTrafficTypes = combination.Segment1.HasTrafficOverlap(combination.Segment2, intersection, context);
                return (
                    combination.Segment1,
                    combination.Segment2,
                    combination.Key,
                    Intersection: intersection,
                    HasOverlapInTrafficTypes: hasOverlapInTrafficTypes
                );
            })
            .Where(x => x.Intersection is not null)
            .ToArray();

        // Validate and create/remove grade junctions
        foreach (var combination in validSegmentCombinations)
        {
            var gradeSeparatedJunctionExists = existingGradeSeparatedJunctions.ContainsKey(combination.Key);
            if (!gradeSeparatedJunctionExists && combination.HasOverlapInTrafficTypes)
            {
                var problemContext = ProblemContext.For(context.IdTranslator.TranslateToTemporaryId(combination.Segment1.RoadSegmentId));
                problems += new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(context.IdTranslator.TranslateToTemporaryId(combination.Segment2.RoadSegmentId))
                    .WithContext(problemContext);
            }

            var gradeJunctionExists = existingGradeJunctions.ContainsKey(combination.Key);
            if (!combination.HasOverlapInTrafficTypes && !gradeJunctionExists)
            {
                problems += AddGradeJunction(combination.Segment1.RoadSegmentId, combination.Segment2.RoadSegmentId, idGenerator, context, summary.GradeJunctions);
            }
            else if (combination.HasOverlapInTrafficTypes && gradeJunctionExists)
            {
                var existingJunction = existingGradeJunctions[combination.Key];
                problems += RemoveGradeJunction(existingJunction.GradeJunctionId, context, summary.GradeJunctions);
            }
        }

        //TODO-pr remove gradejunctions waarvan het segment of de intersection niet meer bestaat

        return problems;
    }

    private static (int, int) CreateCombinationKey(RoadSegmentId id1, RoadSegmentId id2)
    {
        var min = Math.Min(id1.ToInt32(), id2.ToInt32());
        var max = Math.Max(id1.ToInt32(), id2.ToInt32());
        return (min, max);
    }
}
