namespace RoadRegistry.BackOffice.Messages;

public class ModifyGradeSeparatedJunction
{
    public int Id { get; set; }
    public int LowerSegmentId { get; set; }
    public string Type { get; set; }
    public int UpperSegmentId { get; set; }
}
