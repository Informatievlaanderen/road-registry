namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkChangesArchiveUploaded")]
[EventDescription("Indicates the road network changes archive was uploaded.")]
public class RoadNetworkChangesArchiveUploaded : IMessage, IWhen
{
    public string ArchiveId { get; set; }
    public string Description { get; set; }
    public Guid? DownloadId { get; set; }
    public Guid? TicketId { get; set; }
    public string When { get; set; }
}
