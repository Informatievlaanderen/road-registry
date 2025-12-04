namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentStreetNameNotProposedOrCurrent : Error
{
    public RoadSegmentStreetNameNotProposedOrCurrent()
        : base(ProblemCode.RoadSegment.StreetName.NotProposedOrCurrent)
    {
    }
}

public class RoadSegmentLeftStreetNameNotProposedOrCurrent : Error
{
    public RoadSegmentLeftStreetNameNotProposedOrCurrent(RoadSegmentId roadSegmentId)
        : base(ProblemCode.RoadSegment.StreetName.Left.NotProposedOrCurrent, new ProblemParameter("SegmentId", roadSegmentId.ToString()))
    {
    }
}

public class RoadSegmentRightStreetNameNotProposedOrCurrent : Error
{
    public RoadSegmentRightStreetNameNotProposedOrCurrent(RoadSegmentId roadSegmentId)
        : base(ProblemCode.RoadSegment.StreetName.Right.NotProposedOrCurrent, new ProblemParameter("SegmentId", roadSegmentId.ToString()))
    {
    }
}

