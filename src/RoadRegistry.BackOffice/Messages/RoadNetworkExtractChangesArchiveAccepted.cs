namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkExtractChangesArchiveAccepted")]
[EventDescription("Indicates the road network extract changes archive was accepted.")]
public class RoadNetworkExtractChangesArchiveAccepted : IMessage
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public FileProblem[] Problems { get; set; }
    public string RequestId { get; set; }
    public Guid UploadId { get; set; }
    public string Description { get; set; }
    public string When { get; set; }
}
