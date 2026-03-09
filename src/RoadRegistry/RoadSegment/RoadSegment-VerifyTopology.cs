namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems VerifyTopology(ScopedRoadNetworkContext context)
    {
        var idReference = context.IdTranslator.TranslateToTemporaryId(RoadSegmentId);
        var problems = Problems.WithContext(idReference);

        if (IsRemoved || Attributes.Status != RoadSegmentStatusV2.Gerealiseerd)
        {
            return problems;
        }

        var line = Geometry.Value.GetSingleLineString();

        var byOtherSegment = context.RoadNetwork.GetNonRemovedRoadSegments().FirstOrDefault(segment =>
            segment.RoadSegmentId != RoadSegmentId &&
            segment.Geometry.Value.IsReasonablyEqualTo(Geometry.Value, context.Tolerances));
        if (byOtherSegment is not null)
        {
            problems += new RoadSegmentGeometryTaken(context.IdTranslator.TranslateToTemporaryId(byOtherSegment.RoadSegmentId));
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(StartNodeId, out var startNode) || startNode.IsRemoved)
        {
            problems += new RoadSegmentStartNodeMissing();
        }
        else
        {
            if (!line.StartPoint.IsReasonablyEqualTo(startNode.Geometry.Value, context.Tolerances))
            {
                problems += new RoadSegmentStartPointDoesNotMatchNodeGeometry();
            }
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(EndNodeId, out var endNode) || endNode.IsRemoved)
        {
            problems += new RoadSegmentEndNodeMissing();
        }
        else
        {
            if (!line.EndPoint.IsReasonablyEqualTo(endNode.Geometry.Value, context.Tolerances))
            {
                problems += new RoadSegmentEndPointDoesNotMatchNodeGeometry();
            }
        }

        if (!problems.Any())
        {
            //TODO-pr te bekijken indien performance issues bij grote uploads, vroeger werdt hier aparte scopedview voor gemaakt
            var intersectingSegments = FindIntersectingRoadSegments(context.RoadNetwork, RoadSegmentId, Geometry.Value, [StartNodeId, EndNodeId]);

            var duplicateIntersectionsRoadSegmentIds = intersectingSegments
                .Where(x => x.IntersectingCoordinates?.Length > 1)
                .Select(x => x.RoadSegment.RoadSegmentId)
                .ToList();
            if (duplicateIntersectionsRoadSegmentIds.Any())
            {
                foreach (var duplicateIntersectionsRoadSegmentId in duplicateIntersectionsRoadSegmentIds)
                {
                    problems += new RoadSegmentDuplicateIntersections(context.IdTranslator.TranslateToTemporaryId(duplicateIntersectionsRoadSegmentId));
                }
            }

            var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegments
                .Where(intersectingSegment =>
                    !context.RoadNetwork.GradeSeparatedJunctions.Values.Any(junction =>
                        (junction.LowerRoadSegmentId == RoadSegmentId && junction.UpperRoadSegmentId == intersectingSegment.RoadSegment.RoadSegmentId)
                        ||
                        (junction.LowerRoadSegmentId == intersectingSegment.RoadSegment.RoadSegmentId && junction.UpperRoadSegmentId == RoadSegmentId)
                    ))
                .Select(i =>
                    new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                        context.IdTranslator.TranslateToTemporaryId(i.RoadSegment.RoadSegmentId)));

                problems += intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions;
        }

        return problems;
    }

    private static IReadOnlyCollection<(RoadSegment RoadSegment, Coordinate[]? IntersectingCoordinates)> FindIntersectingRoadSegments(
        ScopedRoadNetwork roadNetwork,
        RoadSegmentId intersectsWithId,
        MultiLineString intersectsWithGeometry,
        RoadNodeId[] roadNodeIdsNotInCommon)
    {
        return roadNetwork
            .GetNonRemovedRoadSegments()
            .Where(segment => segment.Attributes.Status == RoadSegmentStatusV2.Gerealiseerd)
            .Where(segment => segment.RoadSegmentId != intersectsWithId)
            .Where(segment => !segment.GetNodeIds().Any(roadNodeIdsNotInCommon.Contains))
            .Select(segment =>
            {
                var intersection = segment.Geometry.Value.Intersection(intersectsWithGeometry);
                return (segment, intersection?.Coordinates);
            })
            .ToList();
    }
}
