namespace RoadRegistry.Editor.Schema.RoadNetworkChanges;

using System;

public class RoadNetworkChangesArchiveUploadedEntry
{
    public ArchiveInfo Archive { get; set; }
    public Guid? TicketId { get; set; }
}
