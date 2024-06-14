namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class AddRoadSegmentToEuropeanRoad : IMessage
{
    public int SegmentId { get; set; }
    public string SegmentGeometryDrawMethod { get; set; }
    public string Number { get; set; }
    public int TemporaryAttributeId { get; set; }
}
