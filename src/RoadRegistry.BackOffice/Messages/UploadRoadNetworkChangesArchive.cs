namespace RoadRegistry.BackOffice.Messages;

using System;

public class UploadRoadNetworkChangesArchive
{
    public string ArchiveId { get; set; }
    public Guid? TicketId { get; set; }
}
