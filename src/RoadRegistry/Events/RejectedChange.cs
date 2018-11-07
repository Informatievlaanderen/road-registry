namespace RoadRegistry.Events
{
    using RoadRegistry.Shared;
    public class RejectedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
        public int Id { get; set; }
        public string Reason { get; set; }
        public ReasonParameter[] Parameters { get; set; }
    }
}
