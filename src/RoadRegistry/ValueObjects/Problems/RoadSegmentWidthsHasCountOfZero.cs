namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentWidthsHasCountOfZero : Error
{
    public RoadSegmentWidthsHasCountOfZero(int identifier)
        : base(ProblemCode.RoadSegment.Widths.HasCountOfZero,
        new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
