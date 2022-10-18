namespace RoadRegistry.BackOffice.Messages;

public class AddGradeSeparatedJunction
{
    public int LowerSegmentId { get; set; }
    public int TemporaryId { get; set; }
    public string Type { get; set; }
    public int UpperSegmentId { get; set; }
}