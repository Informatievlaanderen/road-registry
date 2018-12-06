namespace RoadRegistry.Messages
{
    public class AcceptedChange
    {
        public RoadNodeAdded RoadNodeAdded { get; set; }
        public RoadSegmentAdded RoadSegmentAdded { get; set; }
        public Reason[] Warnings { get; set; }
    }
}
