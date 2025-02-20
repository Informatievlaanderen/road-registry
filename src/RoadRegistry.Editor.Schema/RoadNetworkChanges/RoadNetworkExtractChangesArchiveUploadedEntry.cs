namespace RoadRegistry.Editor.Schema.RoadNetworkChanges;

using System;

public class RoadNetworkExtractChangesArchiveUploadedEntry
{
    public ArchiveInfo Archive { get; set; }

    public Guid? TicketId { get; set; }
}
