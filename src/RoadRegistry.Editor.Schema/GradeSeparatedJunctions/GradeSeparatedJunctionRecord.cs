namespace RoadRegistry.Editor.Schema.GradeSeparatedJunctions;

public class GradeSeparatedJunctionRecord
{
    public int Id { get; set; }
    public int UpperRoadSegmentId { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public byte[] DbaseRecord { get; set; }
}
