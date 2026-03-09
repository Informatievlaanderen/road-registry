namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentGeometryVerticesTooClose : Error
{
    public RoadSegmentGeometryVerticesTooClose()
        : base(ProblemCode.RoadSegment.Geometry.VerticesTooClose.ToString())
    {
    }

    public RoadSegmentGeometryVerticesTooClose(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.Geometry.VerticesTooClose,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
