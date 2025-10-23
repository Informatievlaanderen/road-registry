namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class FakeRoadNodeConnectedSegmentsDoNotDiffer : Warning
{
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
