namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentEuropeanRoadAttributeRecord
    {
        public int Id { get; set; }
        public int RoadSegmentId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
