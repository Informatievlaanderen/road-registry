namespace RoadRegistry.Projections
{
    public class RoadSegmentNationalRoadAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
