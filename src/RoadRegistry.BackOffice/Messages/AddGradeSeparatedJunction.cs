namespace RoadRegistry.BackOffice.Messages
{
    public class AddGradeSeparatedJunction
    {
        public int TemporaryId { get; set; }
        public int UpperSegmentId { get; set; }
        public int LowerSegmentId { get; set; }
        public string Type { get; set; }
    }
}
