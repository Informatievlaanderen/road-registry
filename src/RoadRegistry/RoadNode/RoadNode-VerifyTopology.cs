namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using System.Linq;
using Errors;
using Extensions;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment;
using ScopedRoadNetwork.ValueObjects;

public partial class RoadNode
{
    public Problems VerifyTopology(RoadNetworkVerifyTopologyContext context)
    {
        var problems = Problems.For(RoadNodeId);

        var segments = context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.StartNodeId == RoadNodeId || x.EndNodeId == RoadNodeId)
            .ToList();

        if (IsRemoved)
        {
            foreach (var segment in segments)
            {
                if (segment.StartNodeId == RoadNodeId)
                {
                    problems += new RoadSegmentStartNodeMissing(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId));
                }

                if (segment.EndNodeId == RoadNodeId)
                {
                    problems += new RoadSegmentEndNodeMissing(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId));
                }
            }

            return problems;
        }

        var byOtherNode =
            context.RoadNetwork.GetNonRemovedRoadNodes().FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.Value.IsReasonablyEqualTo(Geometry.Value, context.Tolerances));
        if (byOtherNode is not null)
        {
            problems = problems.Add(new RoadNodeGeometryTaken(
                context.IdTranslator.TranslateToTemporaryId(byOtherNode.RoadNodeId)
            ));
        }

        problems = context.RoadNetwork.GetNonRemovedRoadSegments()
            .Where(s =>
                segments.All(x => x.RoadSegmentId != s.RoadSegmentId)
                && s.Geometry.Value.IsWithinDistance(Geometry.Value, Distances.TooClose)
            )
            .Aggregate(problems, (current, segment) =>
                    current.Add(new RoadNodeTooClose(context.IdTranslator.TranslateToTemporaryId(segment.RoadSegmentId))));

        problems += VerifyTypeMatchesConnectedSegmentCount(context, segments);

        return problems;
    }

    private Problems VerifyTypeMatchesConnectedSegmentCount(RoadNetworkVerifyTopologyContext context, List<RoadSegment> segments)
    {
        //TODO-pr bij upload mee fixen + uncomment unit test VerifyTopologyTests
        var problems = Problems.None;

        if (segments.Count == 0)
        {
            problems += new RoadNodeNotConnectedToAnySegment(context.IdTranslator.TranslateToTemporaryId(RoadNodeId));
        }
        else if (segments.Count == 1 && Type != RoadNodeTypeV2.Eindknoop)
        {
            problems += RoadNodeTypeV2Mismatch.New(
                context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                segments.Select(x => context.IdTranslator.TranslateToTemporaryId(x.RoadSegmentId)).ToArray(),
                Type,
                [RoadNodeTypeV2.Eindknoop]);
        }
        else if (segments.Count == 2)
        {
            if (!Type.IsAnyOf(RoadNodeTypeV2.Schijnknoop))
            {
                problems += RoadNodeTypeV2Mismatch.New(
                    context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                    segments.Select(x => context.IdTranslator.TranslateToTemporaryId(x.RoadSegmentId)).ToArray(),
                    Type,
                    [RoadNodeTypeV2.Schijnknoop]);
            }
            else if (Type == RoadNodeTypeV2.Schijnknoop)
            {
                var segment1 = segments[0];
                var segment2 = segments[1];
                if (segment1.Attributes.Equals(segment2.Attributes))
                {
                    problems += new FakeRoadNodeConnectedSegmentsDoNotDiffer(
                        context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                        context.IdTranslator.TranslateToTemporaryId(segment1.RoadSegmentId),
                        context.IdTranslator.TranslateToTemporaryId(segment2.RoadSegmentId)
                    );
                }
            }
        }
        else if (segments.Count > 2 && !Type.IsAnyOf(RoadNodeTypeV2.EchteKnoop))
        {
            problems += RoadNodeTypeV2Mismatch.New(
                context.IdTranslator.TranslateToTemporaryId(RoadNodeId),
                segments.Select(x => context.IdTranslator.TranslateToTemporaryId(x.RoadSegmentId)).ToArray(),
                Type,
                [RoadNodeTypeV2.EchteKnoop]);
        }

        return problems;
    }
}
