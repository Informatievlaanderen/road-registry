namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentGeometryTaken : Error
{
    public RoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        : base(ProblemCode.RoadSegment.Geometry.Taken,
            new ProblemParameter(
                "ByOtherSegment",
                byOtherSegment.ToInt32().ToString()))
    {
    }
}
