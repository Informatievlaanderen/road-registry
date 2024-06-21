namespace RoadRegistry.Integration.Schema.GradeSeparatedJunctions;

public class GradeSeparatedJunctionLatestItem
{
    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public int UpperRoadSegmentId { get; set; }
}
