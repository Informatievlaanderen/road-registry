namespace RoadRegistry.Messages
{
    public class RejectedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
        public Problem[] Errors { get; set; }
        public Problem[] Warnings { get; set; }
    }
}
