namespace RoadRegistry.RoadNode;

using System.Linq;
using BackOffice;
using BackOffice.Core;
using NetTopologySuite.Geometries;
using RoadNetwork.ValueObjects;
using RoadSegment.ValueObjects;

public partial class RoadNode : MartenAggregateRootEntity<RoadNodeId>
{
    public Problems VerifyTopologyAfterChanges(RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        // if (IsRemoved)
        // {
        //     //TODO-pr implement
        // }

        var byOtherNode =
            context.RoadNetwork.RoadNodes.Values.FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
        if (byOtherNode != null)
            problems = problems.Add(new RoadNodeGeometryTaken(
                context.IdTranslator.TranslateToTemporaryId(byOtherNode.RoadNodeId)
            ));

        var node = context.RoadNetwork.RoadNodes[RoadNodeId];

        problems = context.RoadNetwork.RoadSegments.Values
            .Where(s =>
                !node.Segments.Contains(s.RoadSegmentId) &&
                s.Geometry.IsWithinDistance(Geometry, Distances.TooClose)
            )
            .Aggregate(
                problems,
                (current, segment) =>
                    current.Add(new RoadNodeTooClose(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId))));

        problems = problems.AddRange(node.VerifyTypeMatchesConnectedSegmentCount(context));

        return problems;
    }

    public Problems VerifyTypeMatchesConnectedSegmentCount(RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        if (Segments.Count == 0)
        {
            problems = problems.Add(new RoadNodeNotConnectedToAnySegment(context.IdTranslator.TranslateToTemporaryId(RoadNodeId)));
        }
        else if (Segments.Count == 1 && Type != RoadNodeType.EndNode)
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(context.IdTranslator.TranslateToTemporaryId(RoadNodeId), Segments.Select(context.IdTranslator.TranslateToTemporaryId).ToArray(), Type, [RoadNodeType.EndNode]));
        }
        else if (Segments.Count == 2)
        {
            if (!Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
            {
                problems = problems.Add(RoadNodeTypeMismatch.New(context.IdTranslator.TranslateToTemporaryId(RoadNodeId), Segments.Select(context.IdTranslator.TranslateToTemporaryId).ToArray(), Type, [RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode]));
            }
            else if (Type == RoadNodeType.FakeNode)
            {
                var segments = Segments.Select(segmentId => context.RoadNetwork.RoadSegments[segmentId])
                    .ToArray();
                var segment1 = segments[0];
                var segment2 = segments[1];
                if (segment1.Attributes.Equals(segment2.Attributes))
                {
                    problems = problems.Add(new FakeRoadNodeConnectedSegmentsDoNotDiffer(
                        context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                        context.IdTranslator.TranslateToTemporaryId(segment1.RoadSegmentId),
                        context.IdTranslator.TranslateToTemporaryId(segment2.RoadSegmentId)
                    ));
                }
            }
        }
        else if (Segments.Count > 2 && !Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(context.IdTranslator.TranslateToTemporaryId(RoadNodeId), Segments.Select(context.IdTranslator.TranslateToTemporaryId).ToArray(), Type, [RoadNodeType.RealNode, RoadNodeType.MiniRoundabout]));
        }

        return problems;
    }

    public void ConnectWith(RoadSegmentId segment)
    {
        _segments.Add(segment);
    }
    public void DisconnectFrom(RoadSegmentId segment)
    {
        _segments.Remove(segment);
    }
}
