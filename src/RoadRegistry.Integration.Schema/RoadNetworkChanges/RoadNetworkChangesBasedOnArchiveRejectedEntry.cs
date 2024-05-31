namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

public class RoadNetworkChangesBasedOnArchiveRejectedEntry
{
    public ArchiveInfo Archive { get; set; }
    public RejectedChange[] Changes { get; set; }
}