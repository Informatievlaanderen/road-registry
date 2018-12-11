namespace RoadRegistry.Messages
{
    using Aiv.Vbr.EventHandling;

    [EventName("ImportedGradeSeparatedJunction")]
    [EventDescription("Indicates a road network grade separated junction was imported.")]
    public class ImportedGradeSeparatedJunction
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int UpperRoadSegmentId { get; set; }
        public int LowerRoadSegmentId { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
