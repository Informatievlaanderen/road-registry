namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentEndNodeMissing : Error
{
    public RoadSegmentEndNodeMissing(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.EndNode.Missing,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
