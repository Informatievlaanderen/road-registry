namespace RoadRegistry.ValueObjects.Problems;

using System.Linq;
using ProblemCodes;
using RoadRegistry.Extensions;

public class RoadNodeTooClose : Warning
{
    public RoadNodeTooClose(RoadSegmentIdReference otherSegment)
        : base(ProblemCode.RoadNode.TooClose.ToString(),
            otherSegment.ToRoadSegmentProblemParameters("OtherSegment").ToArray())
    {
    }

    public RoadNodeTooClose(RoadSegmentId toOtherSegment)
        : base(ProblemCode.RoadNode.TooClose,
            new ProblemParameter(
                "ToOtherSegment",
                toOtherSegment.ToInt32().ToString()))
    {
    }
}
