namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentEndNodeMissing : Error
{
    public RoadSegmentEndNodeMissing()
        : base(ProblemCode.RoadSegment.EndNode.Missing)
    {
    }
}
