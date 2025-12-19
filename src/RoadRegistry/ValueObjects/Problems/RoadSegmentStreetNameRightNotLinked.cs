namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentStreetNameRightNotLinked : Error
{
    public RoadSegmentStreetNameRightNotLinked(int wegsegmentId, string rechterstraatnaamId)
        : base(ProblemCode.RoadSegment.StreetName.Right.NotLinked,
            new ProblemParameter("WegsegmentId", wegsegmentId.ToString()),
            new ProblemParameter("RechterstraatnaamId", rechterstraatnaamId)
        )
    {
    }
}
