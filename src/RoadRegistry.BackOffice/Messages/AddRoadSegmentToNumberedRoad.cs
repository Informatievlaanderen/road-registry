namespace RoadRegistry.BackOffice.Messages
{
    public class AddRoadSegmentToNumberedRoad
    {
        public int TemporaryAttributeId { get; set; }
        public int SegmentId { get; set; }
        public string Ident8 { get; set; }
        public string Direction { get; set; }
        public int Ordinal { get; set; }
    }
}
