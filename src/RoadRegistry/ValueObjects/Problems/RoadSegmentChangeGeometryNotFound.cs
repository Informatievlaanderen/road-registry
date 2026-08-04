namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentChangeGeometryNotFound : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.NotFound;

    public RoadSegmentChangeGeometryNotFound(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
