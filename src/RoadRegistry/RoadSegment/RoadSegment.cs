namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment : MartenAggregateRootEntity<RoadSegmentId>
{
    public Problems VerifyTopology(RoadNetworkVerifyTopologyContext context)
    {
        var problems = Problems.None;

        if (IsRemoved)
        {
            if (context.RoadNetwork.RoadNodes.TryGetValue(StartNodeId, out var beforeStartNode))
                problems = problems.AddRange(
                    beforeStartNode.VerifyTypeMatchesConnectedSegmentCount(context));

            if (context.RoadNetwork.RoadNodes.TryGetValue(EndNodeId, out var beforeEndNode))
                problems = problems.AddRange(
                    beforeEndNode.VerifyTypeMatchesConnectedSegmentCount(context));

            return problems;
        }

        var originalIdOrId = context.IdTranslator.TranslateToTemporaryId(RoadSegmentId);

        var line = Geometry.GetSingleLineString();

        if (Attributes.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId);

            return problems;
        }

        var byOtherSegment = context.RoadNetwork.RoadSegments.Values.FirstOrDefault(segment =>
            segment.RoadSegmentId != RoadSegmentId &&
            segment.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
        if (byOtherSegment is not null)
        {
            problems = problems.Add(new RoadSegmentGeometryTaken(byOtherSegment.RoadSegmentId));
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(StartNodeId, out var startNode))
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(startNode.VerifyTypeMatchesConnectedSegmentCount(context));
            if (line.StartPoint != null && !line.StartPoint.IsReasonablyEqualTo(startNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!context.RoadNetwork.RoadNodes.TryGetValue(EndNodeId, out var endNode))
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(endNode.VerifyTypeMatchesConnectedSegmentCount(context));
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
                        (junction.LowerRoadSegmentId == RoadSegmentId && junction.UpperRoadSegmentId == intersectingSegment.Key) ||
                        (junction.LowerRoadSegmentId == intersectingSegment.Key && junction.UpperRoadSegmentId == RoadSegmentId)))
                .Select(i =>
                    new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                        originalIdOrId,
                        context.IdTranslator.TranslateToTemporaryId(i.Key)));

            problems = problems.AddRange(intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions);
        }

        return problems;
    }

    private static IEnumerable<KeyValuePair<RoadSegmentId, RoadSegment>> FindIntersectingRoadSegments(
        RoadNetwork roadNetwork,
        RoadSegmentId intersectsWithId,
        MultiLineString intersectsWithGeometry,
        RoadNodeId[] roadNodeIdsNotInCommon)
    {
        return roadNetwork
            .RoadSegments
            .Where(segment => segment.Value.Attributes.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            .Where(segment => segment.Key != intersectsWithId)
            .Where(segment => segment.Value.Geometry.Intersects(intersectsWithGeometry))
            .Where(segment => !segment.Value.Nodes.Any(roadNodeIdsNotInCommon.Contains));
    }
}
