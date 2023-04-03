namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentGeometrySelfIntersects : Error
{
    public RoadSegmentGeometrySelfIntersects()
        : base(ProblemCode.RoadSegment.Geometry.SelfIntersects)
    {
    }
}
