namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    public Problems VerifyTopology(RoadNetworkVerifyTopologyContext context)
    {
        var problems = Problems.None;

        if (IsRemoved)
        {
            return problems;
        }

        var originalIdOrId = context.IdTranslator.TranslateToTemporaryId(RoadSegmentId);

        var line = Geometry.GetSingleLineString();

        if (Attributes.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId);

            return problems;
        }

        var byOtherSegment = context.RoadNetwork.GetNonRemovedRoadSegments().FirstOrDefault(segment =>
            segment.RoadSegmentId != RoadSegmentId &&
            segment.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
        if (byOtherSegment is not null)
        {
            problems = problems.Add(new RoadSegmentGeometryTaken(byOtherSegment.RoadSegmentId));
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(StartNodeId, out var startNode) || startNode.IsRemoved)
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        }
        else
        {
            if (line.StartPoint != null && !line.StartPoint.IsReasonablyEqualTo(startNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(EndNodeId, out var endNode) || endNode.IsRemoved)
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        }
        else
        {
            if (line.EndPoint != null && !line.EndPoint.IsReasonablyEqualTo(endNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentEndPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!problems.Any())
        {
            //TODO-pr te bekijken indien performance issues bij grote uploads, vroeger werdt hier aparte scopedview voor gemaakt
            var intersectingSegments = FindIntersectingRoadSegments(context.RoadNetwork, RoadSegmentId, Geometry, [StartNodeId, EndNodeId]);
            var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegments
                .Where(intersectingSegment =>
                    !context.RoadNetwork.GradeSeparatedJunctions.Values.Any(junction =>
                        (junction.LowerRoadSegmentId == RoadSegmentId && junction.UpperRoadSegmentId == intersectingSegment.RoadSegmentId) ||
                        (junction.LowerRoadSegmentId == intersectingSegment.RoadSegmentId && junction.UpperRoadSegmentId == RoadSegmentId)))
                .Select(i =>
                    new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                        originalIdOrId,
                        context.IdTranslator.TranslateToTemporaryId(i.RoadSegmentId)));

            problems = problems.AddRange(intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions);
        }

        return problems;
    }

    private static IEnumerable<RoadSegment> FindIntersectingRoadSegments(
        RoadNetwork roadNetwork,
        RoadSegmentId intersectsWithId,
        MultiLineString intersectsWithGeometry,
        RoadNodeId[] roadNodeIdsNotInCommon)
    {
        return roadNetwork
            .GetNonRemovedRoadSegments()
            .Where(segment => segment.Attributes.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            .Where(segment => segment.RoadSegmentId != intersectsWithId)
            .Where(segment => segment.Geometry.Intersects(intersectsWithGeometry))
            .Where(segment => !segment.Nodes.Any(roadNodeIdsNotInCommon.Contains));
    }
}
