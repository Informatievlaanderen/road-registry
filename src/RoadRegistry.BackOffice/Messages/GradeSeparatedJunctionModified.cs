namespace RoadRegistry.BackOffice.Messages
{
    public class GradeSeparatedJunctionModified
    {
        public int Id { get; set;  }
        public int UpperRoadSegmentId { get; set; }
        public int LowerRoadSegmentId { get; set; }
        public string Type { get; set; }
    }
}
