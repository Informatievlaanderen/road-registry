using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class GradeSeparatedJunctionAdded : IMessage
{
    public int Id { get; set; }
    public int TemporaryId { get; set; }
    public int UpperRoadSegmentId { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public string Type { get; set; }
}
