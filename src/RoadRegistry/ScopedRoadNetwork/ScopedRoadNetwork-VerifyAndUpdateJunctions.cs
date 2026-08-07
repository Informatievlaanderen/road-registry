namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    private Problems VerifyAndUpdateJunctions(IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        using var _ = context.Logger.TimeAction();

        var problems = Problems.None;

        // Get existing junctions
        var existingGradeSeparatedJunctions = _gradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.LowerRoadSegmentId, x.UpperRoadSegmentId), x => x);
        var existingGradeJunctions = _gradeJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.RoadSegmentId1, x.RoadSegmentId2), x => x);

        // Find all unique combinations of changed segments with all gerealiseerde segments
        var segmentCombinationsOfChangedSegments = GetSegmentCombinationsOfChangedSegments(
            existingGradeJunctions.Values.Select(x => (x.RoadSegmentId1, x.RoadSegmentId2)), context.Tolerances);
        if (!segmentCombinationsOfChangedSegments.Any())
        {
            return problems;
        }

        // Remove grade junctions that are not intersecting any segment
        foreach (var combination in segmentCombinationsOfChangedSegments
                     .Where(x => x.IntersectingPoints.Length == 0))
        {
            if (existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
            {
                problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context);
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
            var isFarAwayFromNodes = IsAwayFromTheEndPointsOf(intersection, segment1Geometry, segment2Geometry, context.Tolerances);

            if (!isFarAwayFromNodes)
            {
                if (existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
                {
                    problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context);
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
                var geometry = JunctionGeometry.Create(intersection.Factory.CreatePoint(intersection.Coordinate.RoundToCm()));
                problems += AddGradeJunction(combination.Segment1.RoadSegmentId, combination.Segment2.RoadSegmentId, geometry, idGenerator, context);
            }
            else if (hasOverlapInTrafficTypes && existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
            {
                problems += RemoveGradeJunction(gradeJunction.GradeJunctionId, context);
            }
        }

        if (!problems.HasError())
        {
            RecomputeJunctionGeometries(segmentCombinationsOfChangedSegments, existingGradeJunctions, existingGradeSeparatedJunctions, context);
        }

        return problems;
    }

    // The junction handling of a geometry change, which is deliberately not the one above.
    //
    // Changing a geometry is not a statement about how two roads relate to each other: an intersection that appears is
    // simply recorded as a gelijkgrondse kruising, and one that disappears takes whatever recorded it along - grade or
    // grade separated. Whether the two segments share a traffic type is not judged here, so an operator moving a road
    // is never asked to declare an ongelijkgrondse kruising they did not ask for.
    private Problems VerifyAndUpdateJunctionsAfterGeometryChange(IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkChangeContext context)
    {
        using var _ = context.Logger.TimeAction();

        var problems = Problems.None;

        var existingGradeSeparatedJunctions = _gradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.LowerRoadSegmentId, x.UpperRoadSegmentId), x => x);
        var existingGradeJunctions = _gradeJunctions.Values
            .Where(x => !x.IsRemoved)
            .ToDictionary(x => CreateCombinationKey(x.RoadSegmentId1, x.RoadSegmentId2), x => x);

        var combinations = GetSegmentCombinationsOfChangedSegments(
            existingGradeJunctions.Values.Select(x => (x.RoadSegmentId1, x.RoadSegmentId2))
                .Concat(existingGradeSeparatedJunctions.Values.Select(x => (x.LowerRoadSegmentId, x.UpperRoadSegmentId))),
            context.Tolerances);
        if (combinations.Count == 0)
        {
            return problems;
        }

        // VAL-18: a realized segment may not cross the same realized segment more than once.
        foreach (var combination in combinations.Where(x => x.IntersectingPoints.Length > 1))
        {
            var problemContext = ProblemContext.For(context.IdTranslator.TranslateToTemporaryId(combination.Segment1.RoadSegmentId));
            problems += new RoadSegmentDuplicateIntersections(context.IdTranslator.TranslateToTemporaryId(combination.Segment2.RoadSegmentId))
                .WithContext(problemContext);
        }
        if (problems.HasError())
        {
            return problems;
        }

        foreach (var combination in combinations)
        {
            // An intersection where the two segments meet at a shared road node is a normal connection rather than a
            // crossing, so it counts as no intersection at all.
            var intersection = combination.IntersectingPoints.SingleOrDefault();
            if (intersection is not null && !IsAwayFromTheEndPointsOf(
                    intersection,
                    combination.Segment1.Geometry.Value.GetSingleLineString(),
                    combination.Segment2.Geometry.Value.GetSingleLineString(),
                    context.Tolerances))
            {
                intersection = null;
            }

            if (intersection is null)
            {
                if (existingGradeJunctions.TryGetValue(combination.Key, out var goneGradeJunction))
                {
                    problems += RemoveGradeJunction(goneGradeJunction.GradeJunctionId, context);
                }
                if (existingGradeSeparatedJunctions.TryGetValue(combination.Key, out var goneGradeSeparatedJunction))
                {
                    problems += RemoveGradeSeparatedJunction(new RemoveGradeSeparatedJunctionChange
                    {
                        GradeSeparatedJunctionId = goneGradeSeparatedJunction.GradeSeparatedJunctionId
                    }, context);
                }

                continue;
            }

            // A crossing that is already recorded keeps what records it, either way; only its geometry follows.
            if (existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
            {
                gradeJunction.ChangeGeometry(JunctionGeometry.Create(intersection.Factory.CreatePoint(intersection.Coordinate.RoundToCm())), context.Provenance);
                continue;
            }
            if (existingGradeSeparatedJunctions.TryGetValue(combination.Key, out var gradeSeparatedJunction))
            {
                var geometry = JunctionGeometryCalculator.Calculate(combination.Segment1.Geometry, combination.Segment2.Geometry);
                if (geometry is not null)
                {
                    gradeSeparatedJunction.ChangeGeometry(geometry, context.Provenance);
                }
                continue;
            }

            problems += AddGradeJunction(
                combination.Segment1.RoadSegmentId,
                combination.Segment2.RoadSegmentId,
                JunctionGeometry.Create(intersection.Factory.CreatePoint(intersection.Coordinate.RoundToCm())),
                idGenerator,
                context);
        }

        return problems;
    }

    private static bool IsAwayFromTheEndPointsOf(Point intersection, LineString segment1Geometry, LineString segment2Geometry, VerificationContextTolerances tolerances)
    {
        return intersection.IsFarEnoughAwayFrom(
            [
                segment1Geometry.StartPoint, segment1Geometry.EndPoint,
                segment2Geometry.StartPoint, segment2Geometry.EndPoint
            ],
            tolerances.GeometryTolerance);
    }

    private static void RecomputeJunctionGeometries(
        List<(RoadSegment Segment1, RoadSegment Segment2, (int, int) Key, Point[] IntersectingPoints)> segmentCombinationsOfChangedSegments,
        Dictionary<(int, int), GradeJunction> existingGradeJunctions,
        Dictionary<(int, int), GradeSeparatedJunction> existingGradeSeparatedJunctions,
        ScopedRoadNetworkChangeContext context)
    {
        foreach (var combination in segmentCombinationsOfChangedSegments)
        {
            if (existingGradeJunctions.TryGetValue(combination.Key, out var gradeJunction))
            {
                // The grade junction point is the single non-node intersection already computed for this combination.
                var point = combination.IntersectingPoints.FirstOrDefault();
                if (point is not null)
                {
                    gradeJunction.ChangeGeometry(JunctionGeometry.Create(point.Factory.CreatePoint(point.Coordinate.RoundToCm())), context.Provenance);
                }
            }

            if (existingGradeSeparatedJunctions.TryGetValue(combination.Key, out var gradeSeparatedJunction))
            {
                var geometry = JunctionGeometryCalculator.Calculate(combination.Segment1.Geometry, combination.Segment2.Geometry);
                if (geometry is not null)
                {
                    gradeSeparatedJunction.ChangeGeometry(geometry, context.Provenance);
                }
            }
        }
    }

    // The pairs to evaluate: everything spatially near a changed segment, plus the segment pairs that already carry a
    // junction - those have to be looked at even when they have drifted apart, otherwise the junction is never removed.
    private List<(RoadSegment Segment1, RoadSegment Segment2, (int, int) Key, Point[] IntersectingPoints)> GetSegmentCombinationsOfChangedSegments(
        IEnumerable<(RoadSegmentId RoadSegmentId1, RoadSegmentId RoadSegmentId2)> existingJunctionSegmentPairs,
        VerificationContextTolerances tolerances)
    {
        // Get all "gerealiseerde" segments for comparison
        var allGerealiseerdeSegments = GetNonRemovedRoadSegments()
            .Where(x => x.Status == RoadSegmentStatusV2.Gerealiseerd)
            .ToArray();

        // Get all "gerealiseerde" segments that have changes
        var changedGerealiseerdeSegments = allGerealiseerdeSegments
            .Where(x => x.HasChanges())
            .ToArray();

        if (!changedGerealiseerdeSegments.Any())
        {
            return [];
        }

        var spatialIndex = new STRtree<RoadSegment>();
        foreach (var segment in allGerealiseerdeSegments)
        {
            spatialIndex.Insert(segment.Geometry.Value.EnvelopeInternal, segment);
        }

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

        // 2. Existing junctions where one or both segments changed
        //    (handles the case where a segment moved away and the junction must be removed)
        foreach (var (roadSegmentId1, roadSegmentId2) in existingJunctionSegmentPairs)
        {
            if (!changedSegmentIds.Contains(roadSegmentId1) &&
                !changedSegmentIds.Contains(roadSegmentId2))
                continue;

            if (segmentById.TryGetValue(roadSegmentId1, out var junctionSeg1) &&
                segmentById.TryGetValue(roadSegmentId2, out var junctionSeg2))
            {
                AddPairIfNew(junctionSeg1, junctionSeg2);
            }
        }

        return segmentCombinationsOfChangedSegments;

        void AddPairIfNew(RoadSegment segment1, RoadSegment segment2)
        {
            var key = CreateCombinationKey(segment1.RoadSegmentId, segment2.RoadSegmentId);
            if (!processedPairs.Add(key))
                return;

            var intersectionPoints = GetIntersectionPoints(segment1.Geometry, segment2.Geometry, tolerances);
            segmentCombinationsOfChangedSegments.Add((segment1, segment2, key, intersectionPoints));
        }
    }

    private static Point[] GetIntersectionPoints(RoadSegmentGeometry geometry1, RoadSegmentGeometry geometry2, VerificationContextTolerances tolerances)
    {
        var geometry1EndNodes = geometry1.Value.GetStartAndEndPoints();
        var geometry2EndNodes = geometry2.Value.GetStartAndEndPoints();

        return geometry1.Value.Intersection(geometry2.Value).Coordinates
            .Select(c => geometry1.Value.Factory.CreatePoint(c))
            .Where(intersection =>
            {
                if (geometry1EndNodes.Any(c => intersection.IsReasonablyEqualTo(c, tolerances))
                    && geometry2EndNodes.Any(c => intersection.IsReasonablyEqualTo(c, tolerances)))
                {
                    return false;
                }

                return true;
            })
            .ToArray();
    }

    private static (int, int) CreateCombinationKey(RoadSegmentId id1, RoadSegmentId id2)
    {
        var min = Math.Min(id1.ToInt32(), id2.ToInt32());
        var max = Math.Max(id1.ToInt32(), id2.ToInt32());
        return (min, max);
    }
}
