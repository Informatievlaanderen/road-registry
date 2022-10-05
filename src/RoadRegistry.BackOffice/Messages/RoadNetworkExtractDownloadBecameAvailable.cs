namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractDownloadBecameAvailable")]
[EventDescription("Indicates a road network extract became available.")]
public class RoadNetworkExtractDownloadBecameAvailable : IMessage
{
    public string RequestId { get; set; }
    public string ExternalRequestId { get; set; }
    public Guid DownloadId { get; set; }
    public string ArchiveId { get; set; }
    public string When { get; set; }
    public string Description { get; set; }
}
