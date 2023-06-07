namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractDownloadTimeoutOccurred")]
[EventDescription("Indicates that a timeout occurred while attempting to create the extract, possibly due to a too large area surface.")]
public class RoadNetworkExtractDownloadTimeoutOccurred : IMessage
{
    public string Description { get; set; }
    public string RequestId { get; set; }
    public string ExternalRequestId { get; set; }
    public string When { get; set; }
    public bool IsInformative { get; set; }
}
