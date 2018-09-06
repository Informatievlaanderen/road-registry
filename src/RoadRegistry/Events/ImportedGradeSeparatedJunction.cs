namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;
    using Shared;

    [EventName("ImportedGradeSeparatedJunction")]
    [EventDescription("Indicates a road network grade separated junction was imported.")]
    public class ImportedGradeSeparatedJunction
    {
        public int Id { get; set; }
        // TODO: Version meenemen voor wegknoop en wegsegment
        public GradeSeparatedJunctionType Type { get; set; }
        public int UpperRoadSegmentId { get; set; }
        public int LowerRoadSegmentId { get; set; }
        public OriginProperties Origin { get; set; }
    }
}