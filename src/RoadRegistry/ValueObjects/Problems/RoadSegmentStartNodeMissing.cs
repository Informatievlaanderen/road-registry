namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentStartNodeMissing : Error
{
    public RoadSegmentStartNodeMissing(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.StartNode.Missing,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
