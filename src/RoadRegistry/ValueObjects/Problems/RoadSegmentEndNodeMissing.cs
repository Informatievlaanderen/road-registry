namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentEndNodeMissing : Error
{
    public RoadSegmentEndNodeMissing(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.EndNode.Missing,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
