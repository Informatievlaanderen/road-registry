namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class AnnounceRoadNetworkExtractDownloadTimeoutOccurred : IMessage
{
    public string RequestId { get; set; }
}
