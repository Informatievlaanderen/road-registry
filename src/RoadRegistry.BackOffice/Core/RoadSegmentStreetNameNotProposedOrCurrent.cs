namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentStreetNameNotProposedOrCurrent : Error
{
    public RoadSegmentStreetNameNotProposedOrCurrent()
        : base(ProblemCode.RoadSegment.StreetName.NotProposedOrCurrent)
    {
    }
    public RoadSegmentStreetNameNotProposedOrCurrent(RoadSegmentId roadSegmentId)
        : base(ProblemCode.RoadSegment.StreetName.NotProposedOrCurrent, new ProblemParameter("SegmentId", roadSegmentId.ToString()))
    {
    }
}
