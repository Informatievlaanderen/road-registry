namespace RoadRegistry.BackOffice.Schema.RoadSegmentNationalRoadAttributes
{
    public class RoadSegmentNationalRoadAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
