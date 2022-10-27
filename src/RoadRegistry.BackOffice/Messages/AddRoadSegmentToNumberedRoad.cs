using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegmentToNumberedRoad : IMessage
{
    public string Direction { get; set; }
    public string Number { get; set; }
    public int Ordinal { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
}