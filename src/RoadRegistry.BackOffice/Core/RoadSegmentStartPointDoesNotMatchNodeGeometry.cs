namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentStartPointDoesNotMatchNodeGeometry : Error
{
    public RoadSegmentStartPointDoesNotMatchNodeGeometry()
        : base(ProblemCode.RoadSegment.StartPoint.DoesNotMatchNodeGeometry)
    {
    }
}
