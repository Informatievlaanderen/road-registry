namespace RoadRegistry.BackOffice.Schema.RoadSegmentLaneAttributes
{
    public class RoadSegmentLaneAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
