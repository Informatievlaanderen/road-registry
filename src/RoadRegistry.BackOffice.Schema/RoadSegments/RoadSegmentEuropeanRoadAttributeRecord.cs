namespace RoadRegistry.BackOffice.Schema.RoadSegmentEuropeanRoadAttributes
{
    public class RoadSegmentEuropeanRoadAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
