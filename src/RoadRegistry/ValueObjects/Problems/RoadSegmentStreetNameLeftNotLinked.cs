namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentStreetNameLeftNotLinked : Error
{
    public RoadSegmentStreetNameLeftNotLinked(int wegsegmentId, string linkerstraatnaamId)
        : base(ProblemCode.RoadSegment.StreetName.Left.NotLinked,
            new ProblemParameter("WegsegmentId", wegsegmentId.ToString()),
            new ProblemParameter("LinkerstraatnaamId", linkerstraatnaamId)
        )
    {
    }
}
