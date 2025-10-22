namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class StreetNameNotFound : Error
{
    public StreetNameNotFound()
        : base(ProblemCode.StreetName.NotFound)
    {
    }
}

public class LeftStreetNameNotFound : Error
{
    public LeftStreetNameNotFound(RoadSegmentId roadSegmentId)
        : base(ProblemCode.RoadSegment.StreetName.Left.NotFound, new ProblemParameter("SegmentId", roadSegmentId.ToString()))
    {
    }
}

public class RightStreetNameNotFound : Error
{
    public RightStreetNameNotFound(RoadSegmentId roadSegmentId)
        : base(ProblemCode.RoadSegment.StreetName.Right.NotFound, new ProblemParameter("SegmentId", roadSegmentId.ToString()))
    {
    }
}
