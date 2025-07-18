namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ImportedGradeSeparatedJunction")]
[EventDescription("Indicates a road network grade separated junction was imported.")]
public class ImportedGradeSeparatedJunction : IMessage, IWhen
{
    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public ImportedOriginProperties Origin { get; set; }
    public string Type { get; set; }
    public int UpperRoadSegmentId { get; set; }
    public string When { get; set; }
}
