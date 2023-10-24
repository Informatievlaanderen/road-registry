namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class AddGradeSeparatedJunction : IMessage
{
    public int LowerSegmentId { get; set; }
    public int TemporaryId { get; set; }
    public int? OriginalId { get; set; }
    public string Type { get; set; }
    public int UpperSegmentId { get; set; }
}
