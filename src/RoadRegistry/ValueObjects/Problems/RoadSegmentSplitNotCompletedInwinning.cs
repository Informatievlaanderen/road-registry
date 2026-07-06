namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentSplitNotCompletedInwinning : Error
{
    private static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Split.NotCompletedInwinning;

    public RoadSegmentSplitNotCompletedInwinning(RoadSegmentId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
