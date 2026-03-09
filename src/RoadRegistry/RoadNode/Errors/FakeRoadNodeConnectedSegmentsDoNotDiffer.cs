namespace RoadRegistry.RoadNode.Errors;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

public class FakeRoadNodeConnectedSegmentsDoNotDiffer : Warning
{
    public FakeRoadNodeConnectedSegmentsDoNotDiffer(RoadSegmentIdReference segment1IdReference, RoadSegmentIdReference segment2IdReference)
        : base(ProblemCode.RoadNode.Fake.ConnectedSegmentsDoNotDiffer.ToString(), Enumerable.Empty<ProblemParameter>()
            .Concat(segment1IdReference.ToRoadSegmentProblemParameters("Wegsegment1"))
            .Concat(segment2IdReference.ToRoadSegmentProblemParameters("Wegsegment2"))
            .ToArray()
        )
    {
    }

    public FakeRoadNodeConnectedSegmentsDoNotDiffer(RoadNodeId node, RoadSegmentId segment1, RoadSegmentId segment2)
        : base(ProblemCode.RoadNode.Fake.ConnectedSegmentsDoNotDiffer,
            new ProblemParameter(
                "RoadNodeId",
                node.ToInt32().ToString()),
            new ProblemParameter(
                "SegmentId",
                segment1.ToInt32().ToString()),
            new ProblemParameter(
                "SegmentId",
                segment2.ToInt32().ToString())
        )
    {
    }
}
