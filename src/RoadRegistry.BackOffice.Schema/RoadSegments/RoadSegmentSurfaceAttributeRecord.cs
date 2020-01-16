namespace RoadRegistry.BackOffice.Schema.RoadSegmentSurfaceAttributes
{
    public class RoadSegmentSurfaceAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
