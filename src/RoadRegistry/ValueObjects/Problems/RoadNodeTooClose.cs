namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

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
