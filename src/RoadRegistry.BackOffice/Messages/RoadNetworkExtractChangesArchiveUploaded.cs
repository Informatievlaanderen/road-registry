namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractChangesArchiveUploaded")]
[EventDescription("Indicates the road network extract changes archive was uploaded.")]
public class RoadNetworkExtractChangesArchiveUploaded : IMessage
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public Guid UploadId { get; set; }
    public string Description { get; set; }
    public Guid? TicketId { get; set; }
    public string When { get; set; }
}
