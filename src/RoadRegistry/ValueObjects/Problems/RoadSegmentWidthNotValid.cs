namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentWidthNotValid : Error
{
    public RoadSegmentWidthNotValid(RoadSegmentWidth width)
        : base(ProblemCode.RoadSegment.Width.NotValid,
        new ProblemParameter("Width", width.ToDutchString()))
    {
    }
}
