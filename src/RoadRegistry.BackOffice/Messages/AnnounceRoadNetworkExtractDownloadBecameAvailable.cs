namespace RoadRegistry.BackOffice.Messages;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.EventHandling;

public class AnnounceRoadNetworkExtractDownloadBecameAvailable : IMessage
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public bool IsInformative { get; set; }
    public string RequestId { get; set; }
    public string ZipArchiveWriterVersion { get; set; }
    public ICollection<Guid> OverlapsWithDownloadIds { get; set; }
}
