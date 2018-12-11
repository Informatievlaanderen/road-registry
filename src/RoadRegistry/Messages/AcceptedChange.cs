namespace RoadRegistry.Messages
{
    public class AcceptedChange
    {
        public RoadNodeAdded RoadNodeAdded { get; set; }
        public RoadSegmentAdded RoadSegmentAdded { get; set; }
        public RoadSegmentAddedToEuropeanRoad RoadSegmentAddedToEuropeanRoad { get; set; }
        public RoadSegmentAddedToNationalRoad RoadSegmentAddedToNationalRoad { get; set; }
        public RoadSegmentAddedToNumberedRoad RoadSegmentAddedToNumberedRoad { get; set; }
        public GradeSeparatedJunctionAdded GradeSeparatedJunctionAdded { get; set; }
        public Problem[] Warnings { get; set; }
    }
}
