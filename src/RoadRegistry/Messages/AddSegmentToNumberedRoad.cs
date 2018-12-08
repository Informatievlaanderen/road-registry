namespace RoadRegistry.Messages
{
    public class AddSegmentToNumberedRoad
    {
        public int TemporaryAttributeId { get; set; }
        public string Ident8 { get; set; }
        public int RoadSegmentId { get; set; }
        public string Direction { get; set; }
        public int Ordinal { get; set; }
    }
}