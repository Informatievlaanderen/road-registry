namespace RoadRegistry.BackOffice.Schema.RoadSegmentWidthAttributes
{
    public class RoadSegmentWidthAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
