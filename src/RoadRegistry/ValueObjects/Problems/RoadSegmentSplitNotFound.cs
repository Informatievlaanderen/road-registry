namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentSplitNotFound : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Split.NotFound;

    public RoadSegmentSplitNotFound(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
