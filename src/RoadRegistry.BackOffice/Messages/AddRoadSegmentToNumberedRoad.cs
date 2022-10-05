using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegmentToNumberedRoad : IMessage
{
    public int TemporaryAttributeId { get; set; }
    public int SegmentId { get; set; }
    public string Number { get; set; }
    public string Direction { get; set; }
    public int Ordinal { get; set; }
}
