namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentEndPointDoesNotMatchNodeGeometry : Error
{
    public RoadSegmentEndPointDoesNotMatchNodeGeometry(RoadSegmentId identifier)
        : base(ProblemCode.RoadSegment.EndPoint.DoesNotMatchNodeGeometry,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
