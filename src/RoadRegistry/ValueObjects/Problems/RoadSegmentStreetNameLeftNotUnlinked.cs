namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentStreetNameLeftNotUnlinked : Error
{
    public RoadSegmentStreetNameLeftNotUnlinked(int wegsegmentId)
        : base(ProblemCode.RoadSegment.StreetName.Left.NotUnlinked,
            new ProblemParameter("WegsegmentId", wegsegmentId.ToString()))
    {
    }
}
