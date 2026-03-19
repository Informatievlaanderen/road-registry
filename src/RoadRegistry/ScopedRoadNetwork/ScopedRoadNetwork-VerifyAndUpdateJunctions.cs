namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
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

        // Build a spatial index over all gerealiseerde segments to avoid O(n²) intersection tests.
        var spatialIndex = new STRtree<RoadSegment>();
        foreach (var segment in allGerealiseerdeSegments)
        {
            spatialIndex.Insert(segment.Geometry.Value.EnvelopeInternal, segment);
        }

        // Get existing junctions (needed early to supplement the spatial index for segments that moved apart)
        var existingGradeSeparatedJunctions = _gradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.LowerRoadSegmentId, x.UpperRoadSegmentId), x => x);
        var existingGradeJunctions = _gradeJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.RoadSegmentId1, x.RoadSegmentId2), x => x);

        // Build helpers for looking up segments and tracking which changed
        var changedSegmentIds = new HashSet<RoadSegmentId>(changedGerealiseerdeSegments.Select(x => x.RoadSegmentId));
        var segmentById = allGerealiseerdeSegments.ToDictionary(x => x.RoadSegmentId);

        // Find all unique pairs to evaluate.
        // The spatial index pre-filters candidates by bounding-box overlap.
        // Existing grade junction pairs involving a changed segment are also included
        // so that junctions whose segments have moved apart are correctly removed.
        var processedPairs = new HashSet<(int, int)>();
        var segmentCombinationsOfChangedSegments = new List<(
            RoadSegment Segment1,
            RoadSegment Segment2,
            (int, int) Key,
            Point[] IntersectingPoints
        )>();

        void AddPairIfNew(RoadSegment segment1, RoadSegment segment2)
        {
            var key = CreateCombinationKey(segment1.RoadSegmentId, segment2.RoadSegmentId);
            if (!processedPairs.Add(key))
                return;

            var intersectionPoints = GetIntersectionPoints(segment1.Geometry, segment2.Geometry);
            segmentCombinationsOfChangedSegments.Add((segment1, segment2, key, intersectionPoints));
        }

        // 1. Spatially nearby pairs using the spatial index
        foreach (var changed in changedGerealiseerdeSegments)
        {
            var candidates = spatialIndex.Query(changed.Geometry.Value.EnvelopeInternal);
            foreach (var other in candidates)
            {
                if (changed.RoadSegmentId == other.RoadSegmentId)
                    continue;

                AddPairIfNew(changed, other);
            }
        }

        // 2. Existing grade junctions where one or both segments changed
        //    (handles the case where a segment moved away and the junction must be removed)
        foreach (var junction in existingGradeJunctions.Values)
        {
            if (!changedSegmentIds.Contains(junction.RoadSegmentId1) &&
                !changedSegmentIds.Contains(junction.RoadSegmentId2))
                continue;

            if (segmentById.TryGetValue(junction.RoadSegmentId1, out var junctionSeg1) &&
                segmentById.TryGetValue(junction.RoadSegmentId2, out var junctionSeg2))
            {
                AddPairIfNew(junctionSeg1, junctionSeg2);
            }
        }

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
                if (existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
                {
                    problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context, summary);
                }
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
            else if (hasOverlapInTrafficTypes && existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
            {
                problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context, summary);
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
