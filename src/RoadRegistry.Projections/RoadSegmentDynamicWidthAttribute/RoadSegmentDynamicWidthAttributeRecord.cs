namespace RoadRegistry.Projections
{
    public class RoadSegmentDynamicWidthAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
