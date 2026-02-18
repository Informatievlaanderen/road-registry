namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentGeometrySelfOverlaps : Error
{
    public RoadSegmentGeometrySelfOverlaps(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.Geometry.SelfOverlaps,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
