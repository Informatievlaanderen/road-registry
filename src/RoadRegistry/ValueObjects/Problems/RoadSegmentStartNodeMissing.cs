namespace RoadRegistry.ValueObjects.Problems;

using RoadRegistry.ValueObjects.ProblemCodes;

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
