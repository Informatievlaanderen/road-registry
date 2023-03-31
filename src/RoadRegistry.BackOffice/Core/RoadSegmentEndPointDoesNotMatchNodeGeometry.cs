namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentEndPointDoesNotMatchNodeGeometry : Error
{
    public RoadSegmentEndPointDoesNotMatchNodeGeometry()
        : base(ProblemCode.RoadSegment.EndPoint.DoesNotMatchNodeGeometry)
    {
    }
}
