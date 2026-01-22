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
    public Problems VerifyTopology(RoadNetworkVerifyTopologyContext context)
    {
        var problems = Problems.For(RoadSegmentId);

        if (IsRemoved || Attributes.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingeschetst)
        {
            return problems;
        }

        var originalIdOrId = context.IdTranslator.TranslateToTemporaryId(RoadSegmentId);
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
            problems += new RoadSegmentStartNodeMissing(originalIdOrId);
        }
        else
        {
            if (!line.StartPoint.IsReasonablyEqualTo(startNode.Geometry.Value, context.Tolerances))
            {
                problems += new RoadSegmentStartPointDoesNotMatchNodeGeometry(originalIdOrId);
            }
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(EndNodeId, out var endNode) || endNode.IsRemoved)
        {
            problems += new RoadSegmentEndNodeMissing(originalIdOrId);
        }
        else
        {
            if (!line.EndPoint.IsReasonablyEqualTo(endNode.Geometry.Value, context.Tolerances))
            {
                problems += new RoadSegmentEndPointDoesNotMatchNodeGeometry(originalIdOrId);
            }
        }

        if (!problems.Any())
        {
            //TODO-pr te bekijken indien performance issues bij grote uploads, vroeger werdt hier aparte scopedview voor gemaakt
            var intersectingSegments = FindIntersectingRoadSegments(context.RoadNetwork, RoadSegmentId, Geometry.Value, [StartNodeId, EndNodeId]);
            var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegments
                .Where(intersectingSegment =>
                    !context.RoadNetwork.GradeSeparatedJunctions.Values.Any(junction =>
                        (junction.LowerRoadSegmentId == RoadSegmentId && junction.UpperRoadSegmentId == intersectingSegment.RoadSegmentId) ||
                        (junction.LowerRoadSegmentId == intersectingSegment.RoadSegmentId && junction.UpperRoadSegmentId == RoadSegmentId)))
                .Select(i =>
                    new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                        originalIdOrId,
                        context.IdTranslator.TranslateToTemporaryId(i.RoadSegmentId)));

            problems += intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions;
        }

        return problems;
    }

    private static IEnumerable<RoadSegment> FindIntersectingRoadSegments(
        ScopedRoadNetwork roadNetwork,
        RoadSegmentId intersectsWithId,
        MultiLineString intersectsWithGeometry,
        RoadNodeId[] roadNodeIdsNotInCommon)
    {
        return roadNetwork
            .GetNonRemovedRoadSegments()
            .Where(segment => segment.Attributes.GeometryDrawMethod != RoadSegmentGeometryDrawMethodV2.Ingeschetst)
            .Where(segment => segment.RoadSegmentId != intersectsWithId)
            .Where(segment => segment.Geometry.Value.Intersects(intersectsWithGeometry))
            .Where(segment => !segment.Nodes.Any(roadNodeIdsNotInCommon.Contains));
    }
}
