namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentStartPointDoesNotMatchNodeGeometry : Error
{
    public RoadSegmentStartPointDoesNotMatchNodeGeometry(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.StartPoint.DoesNotMatchNodeGeometry,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
