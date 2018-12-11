namespace RoadRegistry.Messages
{
    public class RejectedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
        public AddRoadSegmentToEuropeanRoad AddRoadSegmentToEuropeanRoad { get; set; }
        public AddRoadSegmentToNationalRoad AddRoadSegmentToNationalRoad { get; set; }
        public AddRoadSegmentToNumberedRoad AddRoadSegmentToNumberedRoad { get; set; }
        public Problem[] Errors { get; set; }
        public Problem[] Warnings { get; set; }
    }
}
