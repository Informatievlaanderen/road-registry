namespace RoadRegistry.Editor.Schema.RoadNetworkChanges;

public class RoadNetworkChangesBasedOnArchiveRejectedEntry
{
    public ArchiveInfo Archive { get; set; }
    public RejectedChange[] Changes { get; set; }
}
