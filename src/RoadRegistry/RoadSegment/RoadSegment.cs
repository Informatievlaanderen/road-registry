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

public partial class RoadSegment : AggregateRootEntity
{
    public Problems VerifyTopologyAfterChanges(RoadNetworkChangeContext context)
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

        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);

        var line = Geometry.GetSingleLineString();

        if (AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);

            return problems;
        }

        var byOtherSegment = context.RoadNetwork.RoadSegments.Values.FirstOrDefault(segment =>
            segment.Id != Id &&
            segment.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
        if (byOtherSegment is not null)
        {
            problems = problems.Add(new RoadSegmentGeometryTaken(byOtherSegment.Id));
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

        problems += line.GetProblemsForRoadSegmentLanes(Lanes, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentWidths(Widths, context.Tolerances);
        problems += line.GetProblemsForRoadSegmentSurfaces(Surfaces, context.Tolerances);

        if (!problems.Any())
        {
            //TODO-pr te bekijken indien performance issues bij grote uploads, vroeger werdt hier aparte scopedview voor gemaakt
            var intersectingSegments = FindIntersectingRoadSegments(context.RoadNetwork, Id, Geometry, [StartNodeId, EndNodeId]);
            var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegments
                .Where(intersectingSegment =>
                    !context.RoadNetwork.GradeSeparatedJunctions.Values.Any(junction =>
                        (junction.LowerSegment == Id && junction.UpperSegment == intersectingSegment.Key) ||
                        (junction.LowerSegment == intersectingSegment.Key && junction.UpperSegment == Id)))
                .Select(i =>
                    new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                        originalIdOrId,
                        context.Translator.TranslateToOriginalOrTemporaryOrId(i.Key)));

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
            .Where(segment => segment.Value.AttributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            .Where(segment => segment.Key != intersectsWithId)
            .Where(segment => segment.Value.Geometry.Intersects(intersectsWithGeometry))
            .Where(segment => !segment.Value.Nodes.Any(roadNodeIdsNotInCommon.Contains));
    }
}
