using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class GradeSeparatedJunctionModified : IMessage
{
    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public string Type { get; set; }
    public int UpperRoadSegmentId { get; set; }
}