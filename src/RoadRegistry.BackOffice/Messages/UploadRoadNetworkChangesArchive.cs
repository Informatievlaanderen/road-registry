namespace RoadRegistry.BackOffice.Messages;

using System;

public class UploadRoadNetworkChangesArchive
{
    public string ExtractRequestId { get; set; }
    public string ArchiveId { get; set; }
    public Guid? TicketId { get; set; }
}
