namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentSurfacesHasCountOfZero : Error
{
    public RoadSegmentSurfacesHasCountOfZero(int identifier)
        : base(ProblemCode.RoadSegment.Surfaces.HasCountOfZero,
        new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
