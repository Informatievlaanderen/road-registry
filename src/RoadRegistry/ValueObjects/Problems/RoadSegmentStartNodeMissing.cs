namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentStartNodeMissing : Error
{
    public RoadSegmentStartNodeMissing(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.StartNode.Missing,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
