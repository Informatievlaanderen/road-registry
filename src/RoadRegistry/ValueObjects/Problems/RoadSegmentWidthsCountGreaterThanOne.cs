namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentWidthsCountGreaterThanOne : Error
{
    public RoadSegmentWidthsCountGreaterThanOne(int identifier)
        : base(ProblemCode.RoadSegment.Widths.CountGreaterThanOne,
        new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
