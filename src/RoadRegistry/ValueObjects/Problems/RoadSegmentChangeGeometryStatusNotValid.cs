namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentChangeGeometryStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.StatusNotValid;

    public RoadSegmentChangeGeometryStatusNotValid(RoadSegmentId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
