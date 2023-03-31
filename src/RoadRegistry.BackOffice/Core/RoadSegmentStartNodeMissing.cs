namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentStartNodeMissing : Error
{
    public RoadSegmentStartNodeMissing()
        : base(ProblemCode.RoadSegment.StartNode.Missing)
    {
    }
}
