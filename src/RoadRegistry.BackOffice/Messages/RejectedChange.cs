namespace RoadRegistry.BackOffice.Messages
{
    public class RejectedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public ModifyRoadNode ModifyRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
        public AddRoadSegmentToEuropeanRoad AddRoadSegmentToEuropeanRoad { get; set; }
        public AddRoadSegmentToNationalRoad AddRoadSegmentToNationalRoad { get; set; }
        public AddRoadSegmentToNumberedRoad AddRoadSegmentToNumberedRoad { get; set; }
        public AddGradeSeparatedJunction AddGradeSeparatedJunction { get; set; }
        public Problem[] Problems { get; set; }
    }
}
