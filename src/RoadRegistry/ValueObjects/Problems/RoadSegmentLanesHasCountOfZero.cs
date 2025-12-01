namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentLanesHasCountOfZero : Error
{
    public RoadSegmentLanesHasCountOfZero(int identifier)
        : base(ProblemCode.RoadSegment.Lanes.HasCountOfZero,
        new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
