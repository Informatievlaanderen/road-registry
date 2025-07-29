namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractDownloaded")]
[EventDescription("Indicates a road network extract has been downloaded.")]
public class RoadNetworkExtractDownloaded : IMessage, IWhen
{
    public string Description { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public string When { get; set; }
    public bool IsInformative { get; set; }
}
