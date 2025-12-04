namespace RoadRegistry.BackOffice.Extracts.DbaseV2.RoadSegments;

public class RoadSegmentSurfaceAttributeRecord
{
    public byte[] DbaseRecord { get; set; }
    public int Id { get; set; }
    public int RoadSegmentId { get; set; }
}