namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

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
