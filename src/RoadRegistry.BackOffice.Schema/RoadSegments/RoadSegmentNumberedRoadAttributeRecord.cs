namespace RoadRegistry.BackOffice.Schema.RoadSegmentNumberedRoadAttributes
{
    public class RoadSegmentNumberedRoadAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
