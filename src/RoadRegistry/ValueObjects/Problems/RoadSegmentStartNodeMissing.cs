namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentStartNodeMissing : Error
{
    public RoadSegmentStartNodeMissing()
        : base(ProblemCode.RoadSegment.StartNode.Missing.ToString())
    {
    }

    public RoadSegmentStartNodeMissing(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.StartNode.Missing,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
