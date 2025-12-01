namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadNodeTooClose : Warning
{
    public RoadNodeTooClose(RoadSegmentId toOtherSegment) :
        base(ProblemCode.RoadNode.TooClose,
            new ProblemParameter(
                "ToOtherSegment",
                toOtherSegment.ToInt32().ToString()))
    {
    }
}
