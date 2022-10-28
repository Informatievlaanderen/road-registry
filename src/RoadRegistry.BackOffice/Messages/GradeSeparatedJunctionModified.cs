namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class GradeSeparatedJunctionModified : IMessage
{
    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public string Type { get; set; }
    public int UpperRoadSegmentId { get; set; }
}