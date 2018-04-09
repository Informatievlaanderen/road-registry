namespace RoadRegistry.Commands
{
    public class GradeSeparatedJunction
    {
        public int Id { get; set; }
        public GradeSeparatedJunctionType Type { get; set; }
        public int UpperRoadSegmentId { get; set; }
        public int LowerRoadSegmentId { get; set; }
        public OriginProperties Origin { get; set; }
    }
}