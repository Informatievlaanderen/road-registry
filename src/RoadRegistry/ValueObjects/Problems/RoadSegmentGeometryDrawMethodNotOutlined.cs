namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentGeometryDrawMethodNotOutlined : Error
{
    public RoadSegmentGeometryDrawMethodNotOutlined()
        : base(ProblemCode.RoadSegment.GeometryDrawMethod.NotOutlined)
    {
    }
}
