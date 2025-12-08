namespace RoadRegistry.GradeSeparatedJunction;

public partial class GradeSeparatedJunction
{
    public bool IsConnectedTo(RoadSegmentId roadSegmentId)
    {
        return UpperRoadSegmentId == roadSegmentId || LowerRoadSegmentId == roadSegmentId;
    }
}
