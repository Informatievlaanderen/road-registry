namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentGeometrySelfOverlaps : Error
{
    public RoadSegmentGeometrySelfOverlaps()
        : base(ProblemCode.RoadSegment.Geometry.SelfOverlaps)
    {
    }
}
