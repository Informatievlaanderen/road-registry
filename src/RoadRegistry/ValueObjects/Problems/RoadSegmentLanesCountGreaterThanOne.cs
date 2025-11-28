namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentLanesCountGreaterThanOne : Error
{
    public RoadSegmentLanesCountGreaterThanOne(int identifier)
        : base(ProblemCode.RoadSegment.Lanes.CountGreaterThanOne,
        new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
