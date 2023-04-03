namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentStreetNameNotProposedOrCurrent : Error
{
    public RoadSegmentStreetNameNotProposedOrCurrent()
        : base(ProblemCode.RoadSegment.StreetName.NotProposedOrCurrent)
    {
    }
}
