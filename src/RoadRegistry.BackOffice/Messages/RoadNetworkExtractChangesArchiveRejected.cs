namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractChangesArchiveRejected")]
[EventDescription("Indicates the road network extract changes archive was rejected.")]
public class RoadNetworkExtractChangesArchiveRejected : IMessage, IWhen
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public FileProblem[] Problems { get; set; }
    public string RequestId { get; set; }
    public Guid UploadId { get; set; }
    public string Description { get; set; }
    public Guid? TicketId { get; set; }
    public string When { get; set; }
}
