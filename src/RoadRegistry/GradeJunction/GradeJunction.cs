namespace RoadRegistry.GradeJunction;

public partial class GradeJunction
{
    public bool IsConnectedTo(RoadSegmentId roadSegmentId)
    {
        return RoadSegmentId2 == roadSegmentId || RoadSegmentId1 == roadSegmentId;
    }
}
