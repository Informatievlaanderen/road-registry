namespace RoadRegistry.GradeJunction;

public partial class GradeJunction
{
    public bool IsConnectedTo(RoadSegmentId roadSegmentId)
    {
        return RoadSegmentId1 == roadSegmentId || RoadSegmentId2 == roadSegmentId;
    }
}
