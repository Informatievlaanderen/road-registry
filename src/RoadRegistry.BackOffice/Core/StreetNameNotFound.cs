namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class StreetNameNotFound : Error
{
    public StreetNameNotFound()
        : base(ProblemCode.StreetName.NotFound)
    {
    }

    public StreetNameNotFound(RoadSegmentId roadSegmentId)
        : base(ProblemCode.StreetName.NotFound, new ProblemParameter("SegmentId", roadSegmentId.ToString()))
    {
    }
}
