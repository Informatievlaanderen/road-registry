namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentGeometrySelfIntersects : Error
{
    public RoadSegmentGeometrySelfIntersects()
        : base(ProblemCode.RoadSegment.Geometry.SelfIntersects.ToString())
    {
    }

    public RoadSegmentGeometrySelfIntersects(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.Geometry.SelfIntersects,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
