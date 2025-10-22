namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentEndPointDoesNotMatchNodeGeometry : Error
{
    public RoadSegmentEndPointDoesNotMatchNodeGeometry(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.EndPoint.DoesNotMatchNodeGeometry,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
