namespace RoadRegistry.Messages
{
    public class RequestedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
        public AddSegmentToEuropeanRoad AddSegmentToEuropeanRoad { get; set; }
        public AddSegmentToNationalRoad AddSegmentToNationalRoad { get; set; }
        public AddSegmentToNumberedRoad AddSegmentToNumberedRoad { get; set; }
    }
}
