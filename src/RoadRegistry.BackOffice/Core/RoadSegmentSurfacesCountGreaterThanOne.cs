namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentSurfacesCountGreaterThanOne : Error
{
    public RoadSegmentSurfacesCountGreaterThanOne(int identifier)
        : base(ProblemCode.RoadSegment.Surfaces.CountGreaterThanOne,
        new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
