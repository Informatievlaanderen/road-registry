using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegmentToEuropeanRoad : IMessage
{
    public string Number { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
}