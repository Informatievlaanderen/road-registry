namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public partial class ScopedRoadNetwork
{
    // 'Verwijder wegknoop', which is how road segments are merged: a road node that turns out not to belong there -
    // an unwarranted cut, a road historized away, a node a GRB service placed by mistake - leaves roads that should be
    // one. Removing the node is the same act as joining them, so this is one action rather than two.
    //
    // Two shapes of node can be undone this way. A validatieknoop holds exactly the two roads that become one. An
    // echte knoop with exactly four holds two pairs: walk a circle around the node and the second road you cross is
    // the one the first joins with - the road across from it, not either of its neighbours.
    public RoadNetworkChangeResult RemoveRoadNodeByMergingRoadSegments(
        RoadNodeId roadNodeId,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;
        using var _ = logger.TimeAction();

        var context = new ScopedRoadNetworkChangeContext(this, provenance, logger);

        // VAL-2
        if (!_roadNodes.TryGetValue(roadNodeId, out var roadNode))
        {
            return Failed(new RoadNodeRemoveDoesNotExist(roadNodeId), context);
        }

        // VAL-3
        if (roadNode.IsRemoved)
        {
            return Failed(new RoadNodeRemoveIsRemoved(roadNodeId), context);
        }

        // A road node that has not completed its inwinning is still a V1 node: it carries no type, so there is no
        // saying whether it is one this action may undo at all.
        if (!roadNode.HasMigrated())
        {
            return Failed(new RoadNodeNotCompletedInwinning(roadNodeId), context);
        }

        var connectedRoadSegments = GetNonRemovedRoadSegments()
            .Where(x => x.StartNodeId == roadNodeId || x.EndNodeId == roadNodeId)
            .ToArray();

        // The roads that are to become one carry the attributes the merged road inherits, so a road whose inwinning is
        // not finished has nothing to give it.
        var inwinningProblems = ValidateRoadSegmentsHaveCompletedInwinning(connectedRoadSegments.Select(x => x.RoadSegmentId));
        if (inwinningProblems.HasError())
        {
            return new RoadNetworkChangeResult(inwinningProblems, context.Summary);
        }

        // VAL-4. A road that begins and ends at this node has no other road to become: it is already the whole loop,
        // so a node it hangs off twice is not one this action can undo either.
        var pairs = PairRoadSegments(roadNodeId, roadNode.Type, connectedRoadSegments);
        if (pairs is null)
        {
            return Failed(new RoadNodeCannotBeRemovedByMerging(roadNodeId), context);
        }

        // The geometry checks below query the spatial index for the roads a merged geometry would run into.
        RebuildSpatialIndexes(logger);

        var problems = Problems.None;
        foreach (var (segment1, segment2) in pairs)
        {
            problems += ValidateMergeIsAllowed(segment1, segment2, roadNodeId, context);
        }

        // Merging four road segments only succeeds if both pairs can be merged, so nothing is applied until every pair
        // has been checked.
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        foreach (var (segment1, segment2) in pairs)
        {
            problems += MergeRoadSegments(segment1, segment2, idGenerator, context);
            if (problems.HasError())
            {
                return new RoadNetworkChangeResult(problems, context.Summary);
            }
        }

        // Four roads merged pairwise leaves two roads crossing where the node used to be, and that crossing is now a
        // gelijkgrondse kruising: the same verification the change upload uses records it.
        problems += VerifyAndUpdateJunctions(idGenerator, context);
        if (problems.HasError())
        {
            return new RoadNetworkChangeResult(problems, context.Summary);
        }

        ApplyChangeSummary(context, provenance);

        return new RoadNetworkChangeResult(problems, context.Summary);
    }

    // Which roads become one. Null means the node is not one this action can undo (VAL-4).
    private static IReadOnlyList<(RoadSegment First, RoadSegment Second)>? PairRoadSegments(
        RoadNodeId roadNodeId,
        RoadNodeTypeV2? roadNodeType,
        IReadOnlyList<RoadSegment> connectedRoadSegments)
    {
        if (connectedRoadSegments.Any(x => x.StartNodeId == roadNodeId && x.EndNodeId == roadNodeId))
        {
            return null;
        }

        if (roadNodeType == RoadNodeTypeV2.Validatieknoop && connectedRoadSegments.Count == 2)
        {
            return [(connectedRoadSegments[0], connectedRoadSegments[1])];
        }

        if (roadNodeType == RoadNodeTypeV2.EchteKnoop && connectedRoadSegments.Count == 4)
        {
            // Ordered by the direction each road leaves the node in, so that opposite roads end up two apart - the
            // 'second road you cross walking the circle'.
            var byBearing = connectedRoadSegments
                .OrderBy(x => BearingAwayFromNode(x, roadNodeId))
                .ToArray();

            return [(byBearing[0], byBearing[2]), (byBearing[1], byBearing[3])];
        }

        return null;
    }

    // The direction the road leaves the node in, taken from the vertex next to it.
    private static double BearingAwayFromNode(RoadSegment roadSegment, RoadNodeId roadNodeId)
    {
        var coordinates = roadSegment.Geometry.Value.GetSingleLineString().Coordinates;

        var (at, next) = roadSegment.StartNodeId == roadNodeId
            ? (coordinates[0], coordinates[1])
            : (coordinates[^1], coordinates[^2]);

        return Math.Atan2(next.Y - at.Y, next.X - at.X);
    }

    // VAL-5 through VAL-8: what the merged road would look like, before anything is changed.
    private Problems ValidateMergeIsAllowed(
        RoadSegment segment1,
        RoadSegment segment2,
        RoadNodeId commonNodeId,
        ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        // VAL-5
        if (segment1.Attributes?.GeometryDrawMethod != segment2.Attributes?.GeometryDrawMethod)
        {
            return problems + new RoadSegmentsMergeGeometryDrawMethodNotEqual(segment1.RoadSegmentId, segment2.RoadSegmentId);
        }

        var mergedGeometry = RoadSegmentGeometryHelper.MergeGeometries(segment1, segment2, commonNodeId, context);

        // VAL-6
        if (RoadSegmentGeometryHelper.GetSameStartEndNodeInvalidGeometrySection(mergedGeometry, context.Tolerances) is not null)
        {
            problems += new RoadSegmentsMergeSameStartEndNode(segment1.RoadSegmentId, segment2.RoadSegmentId);
        }

        // VAL-7
        if (mergedGeometry.GetSingleLineString().SelfOverlaps()
            || RoadSegmentGeometryHelper.GetSelfIntersectingInvalidGeometrySection(mergedGeometry, context.Tolerances) is not null)
        {
            problems += new RoadSegmentsMergeSelfIntersecting(segment1.RoadSegmentId, segment2.RoadSegmentId);
        }

        // VAL-8. The roads that are becoming one are not other roads to themselves, and neither is the road on the
        // other side of an echte knoop it is about to be merged with - it is checked as its own pair.
        var otherSegments = _roadSegmentsSpatialIndex.Query(mergedGeometry.EnvelopeInternal)
            .Where(x => x.RoadSegmentId != segment1.RoadSegmentId && x.RoadSegmentId != segment2.RoadSegmentId)
            .Where(x => x.StartNodeId != commonNodeId && x.EndNodeId != commonNodeId);

        foreach (var otherSegment in otherSegments)
        {
            if (RoadSegmentGeometryHelper.GetFirstMultipleIntersectionsInvalidGeometrySection(mergedGeometry, otherSegment.Geometry.Value, context.Tolerances) is not null)
            {
                problems += new RoadSegmentsMergeMultipleIntersections(segment1.RoadSegmentId, segment2.RoadSegmentId, otherSegment.RoadSegmentId);
            }
        }

        return problems;
    }
}
