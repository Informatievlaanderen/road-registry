namespace RoadRegistry.Commands
{
    public class NumberedRoadSegment
    {
        public int SegmentAttributeId { get; set; }
        public int SegmentId { get; set; }
        public int Ordinal { get; set; }
        public NumberedRoadSegmentDirection Direction { get; set; }
        public RoadSegmentOrigin Origin { get; set; }
    }
}