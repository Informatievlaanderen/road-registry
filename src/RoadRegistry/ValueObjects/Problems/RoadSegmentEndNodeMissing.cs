namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentEndNodeMissing : Error
{
    public RoadSegmentEndNodeMissing()
        : base(ProblemCode.RoadSegment.EndNode.Missing.ToString())
    {
    }

    public RoadSegmentEndNodeMissing(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.EndNode.Missing,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
