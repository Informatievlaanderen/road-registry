namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentGeometryLengthIsZero : Error
{
    public RoadSegmentGeometryLengthIsZero(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.Geometry.LengthIsZero,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
