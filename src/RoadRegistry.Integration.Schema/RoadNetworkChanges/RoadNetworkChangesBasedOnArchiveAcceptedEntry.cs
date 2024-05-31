namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

public class RoadNetworkChangesBasedOnArchiveAcceptedEntry
{
    public ArchiveInfo Archive { get; set; }
    public AcceptedChange[] Changes { get; set; }
    public RoadNetworkChangesSummary Summary { get; set; }
}