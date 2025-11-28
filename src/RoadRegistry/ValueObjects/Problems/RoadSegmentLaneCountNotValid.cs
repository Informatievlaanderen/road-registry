namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentLaneCountNotValid : Error
{
    public RoadSegmentLaneCountNotValid(RoadSegmentLaneCount count)
        : base(ProblemCode.RoadSegment.LaneCount.NotValid,
        new ProblemParameter("Count", count.ToDutchString()))
    {
    }
}
