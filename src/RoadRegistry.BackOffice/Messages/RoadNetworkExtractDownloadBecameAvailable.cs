namespace RoadRegistry.BackOffice.Messages;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractDownloadBecameAvailable")]
[EventDescription("Indicates a road network extract became available.")]
public class RoadNetworkExtractDownloadBecameAvailable : IMessage
{
    public string ArchiveId { get; set; }
    public string Description { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public string When { get; set; }
    public bool IsInformative { get; set; }
    public ICollection<Guid> OverlapsWithDownloadIds { get; set; }
}
