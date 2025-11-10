namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using NetTopologySuite.Geometries;
using RoadNetwork.ValueObjects;
using RoadSegment;

public partial class RoadNode
{
    public Problems VerifyTopology(RoadNetworkVerifyTopologyContext context)
    {
        var problems = Problems.None;

        var segments = context.RoadNetwork.RoadSegments.Values
            .Where(x => x.StartNodeId == RoadNodeId || x.EndNodeId == RoadNodeId)
            .ToList();

        if (IsRemoved)
        {
            foreach (var segment in segments)
            {
                if (segment.StartNodeId == RoadNodeId)
                {
                    problems = problems.Add(new RoadSegmentStartNodeMissing(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId)));
                }

                if (segment.EndNodeId == RoadNodeId)
                {
                    problems = problems.Add(new RoadSegmentEndNodeMissing(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId)));
                }
            }

            return problems;
        }

        var byOtherNode =
            context.RoadNetwork.RoadNodes.Values.FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
        if (byOtherNode is not null)
        {
            problems = problems.Add(new RoadNodeGeometryTaken(
                context.IdTranslator.TranslateToTemporaryId(byOtherNode.RoadNodeId)
            ));
        }

        problems = context.RoadNetwork.RoadSegments.Values
            .Where(s =>
                segments.All(x => x.RoadSegmentId != s.RoadSegmentId)
                && s.Geometry.IsWithinDistance(Geometry, Distances.TooClose)
            )
            .Aggregate(problems, (current, segment) =>
                    current.Add(new RoadNodeTooClose(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId))));

        problems = problems.AddRange(VerifyTypeMatchesConnectedSegmentCount(context, segments));

        return problems;
    }

    public Problems VerifyTypeMatchesConnectedSegmentCount(RoadNetworkVerifyTopologyContext context)
    {
        var segments = context.RoadNetwork.RoadSegments.Values
            .Where(x => x.StartNodeId == RoadNodeId || x.EndNodeId == RoadNodeId)
            .ToList();

        return VerifyTypeMatchesConnectedSegmentCount(context, segments);
    }

    private Problems VerifyTypeMatchesConnectedSegmentCount(RoadNetworkVerifyTopologyContext context, List<RoadSegment> segments)
    {
        var problems = Problems.None;

        if (segments.Count == 0)
        {
            problems = problems.Add(new RoadNodeNotConnectedToAnySegment(context.IdTranslator.TranslateToTemporaryId(RoadNodeId)));
        }
        else if (segments.Count == 1 && Type != RoadNodeType.EndNode)
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(
                context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                segments.Select(x => context.IdTranslator.TranslateToTemporaryId(x.RoadSegmentId)).ToArray(),
                Type,
                [RoadNodeType.EndNode]));
        }
        else if (segments.Count == 2)
        {
            if (!Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
            {
                problems = problems.Add(RoadNodeTypeMismatch.New(
                    context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                    segments.Select(x => context.IdTranslator.TranslateToTemporaryId(x.RoadSegmentId)).ToArray(),
                    Type,
                    [RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode]));
            }
            else if (Type == RoadNodeType.FakeNode)
            {
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
        else if (segments.Count > 2 && !Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(
                context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                segments.Select(x => context.IdTranslator.TranslateToTemporaryId(x.RoadSegmentId)).ToArray(),
                Type,
                [RoadNodeType.RealNode, RoadNodeType.MiniRoundabout]));
        }

        return problems;
    }
}
