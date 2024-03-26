namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentGeometrySelfIntersects : Error
{
    public RoadSegmentGeometrySelfIntersects(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.Geometry.SelfIntersects,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
