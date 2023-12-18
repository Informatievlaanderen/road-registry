namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

public class AnnounceRoadNetworkExtractDownloadTimeoutOccurred : IMessage
{
    public string RequestId { get; set; }
    public Guid DownloadId { get; set; }
}
