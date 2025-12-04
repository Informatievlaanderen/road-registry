namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentStreetNameRightNotUnlinked : Error
{
    public RoadSegmentStreetNameRightNotUnlinked(int wegsegmentId)
        : base(ProblemCode.RoadSegment.StreetName.Right.NotUnlinked,
            new ProblemParameter("WegsegmentId", wegsegmentId.ToString()))
    {
    }
}
