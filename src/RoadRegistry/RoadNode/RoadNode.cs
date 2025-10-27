namespace RoadRegistry.RoadNode;

using System;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using RoadNetwork.ValueObjects;

public partial class RoadNode : AggregateRootEntity
{
    public Problems VerifyTopologyAfterChanges(RoadNetworkChangeContext context)
    {
        //TODO-pr implement verifyafter logic
        throw new NotImplementedException();
    }

    public Problems VerifyTypeMatchesConnectedSegmentCount(RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        if (Segments.Count == 0)
        {
            problems = problems.Add(new RoadNodeNotConnectedToAnySegment(context.Translator.TranslateToTemporaryOrId(RoadNodeId)));
        }
        else if (Segments.Count == 1 && Type != RoadNodeType.EndNode)
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(context.Translator.TranslateToTemporaryOrId(RoadNodeId), Segments.Select(context.Translator.TranslateToOriginalOrTemporaryOrId).ToArray(), Type, [RoadNodeType.EndNode]));
        }
        else if (Segments.Count == 2)
        {
            if (!Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
            {
                problems = problems.Add(RoadNodeTypeMismatch.New(context.Translator.TranslateToTemporaryOrId(RoadNodeId), Segments.Select(context.Translator.TranslateToOriginalOrTemporaryOrId).ToArray(), Type, [RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode]));
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
                        context.Translator.TranslateToTemporaryOrId(RoadNodeId),
                        context.Translator.TranslateToOriginalOrTemporaryOrId(segment1.RoadSegmentId),
                        context.Translator.TranslateToOriginalOrTemporaryOrId(segment2.RoadSegmentId)
                    ));
                }
            }
        }
        else if (Segments.Count > 2 && !Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
        {
            problems = problems.Add(RoadNodeTypeMismatch.New(context.Translator.TranslateToTemporaryOrId(RoadNodeId), Segments.Select(context.Translator.TranslateToOriginalOrTemporaryOrId).ToArray(), Type, [RoadNodeType.RealNode, RoadNodeType.MiniRoundabout]));
        }

        return problems;
    }
}
