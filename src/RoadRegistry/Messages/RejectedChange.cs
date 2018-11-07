namespace RoadRegistry.Messages
{
    public class RejectedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
        public Reason[] Reasons { get; set; }
    }
}
