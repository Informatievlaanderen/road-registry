namespace RoadRegistry.ValueObjects.Problems;

using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentGeometryStartEqualsEnd : Error
{
    public RoadSegmentGeometryStartEqualsEnd()
        : base(ProblemCode.RoadSegment.Geometry.StartEqualsEnd.ToString())
    {
    }
}
