namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentGeometryLengthIsZero : Error
{
    public RoadSegmentGeometryLengthIsZero(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.Geometry.LengthIsZero,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
