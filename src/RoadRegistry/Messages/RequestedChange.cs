namespace RoadRegistry.Messages
{
    public class RequestedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
        public AddRoadSegmentToEuropeanRoad AddRoadSegmentToEuropeanRoad { get; set; }
        public AddRoadSegmentToNationalRoad AddRoadSegmentToNationalRoad { get; set; }
        public AddRoadSegmentToNumberedRoad AddRoadSegmentToNumberedRoad { get; set; }
    }
}
