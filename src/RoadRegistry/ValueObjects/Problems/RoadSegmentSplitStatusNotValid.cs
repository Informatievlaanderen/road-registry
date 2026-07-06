namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentSplitStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Split.StatusNotValid;

    public RoadSegmentSplitStatusNotValid(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
