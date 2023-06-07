namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentGeometryLengthIsZero : Error
{
    public RoadSegmentGeometryLengthIsZero()
        : base(ProblemCode.RoadSegment.Geometry.LengthIsZero)
    {
    }
}
