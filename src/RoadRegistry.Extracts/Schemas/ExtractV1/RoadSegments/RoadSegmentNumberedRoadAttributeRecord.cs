namespace RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;

public class RoadSegmentNumberedRoadAttributeRecord
{
    public byte[] DbaseRecord { get; set; }
    public int Id { get; set; }
    public int RoadSegmentId { get; set; }
}
