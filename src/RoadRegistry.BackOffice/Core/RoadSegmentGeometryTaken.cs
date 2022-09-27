namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentGeometryTaken : Error
{
    public RoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        : base(nameof(RoadSegmentGeometryTaken),
            new ProblemParameter(
                "ByOtherSegment",
                byOtherSegment.ToInt32().ToString()))
    {
    }
}
