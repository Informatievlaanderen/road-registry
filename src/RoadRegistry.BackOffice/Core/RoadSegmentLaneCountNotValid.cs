namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentLaneCountNotValid : Error
{
    public RoadSegmentLaneCountNotValid(RoadSegmentLaneCount count)
        : base(ProblemCode.RoadSegment.LaneCount.NotValid,
        new ProblemParameter("Count", count.ToDutchString()))
    {
    }
}
