namespace RoadRegistry.Integration.Schema.GradeSeparatedJunctions;

public class GradeSeparatedJunctionRecord
{
    public byte[] DbaseRecord { get; set; }
    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public int UpperRoadSegmentId { get; set; }
}