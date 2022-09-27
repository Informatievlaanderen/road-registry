namespace RoadRegistry.Editor.Schema.RoadNetworkChanges;

public class RoadNetworkChangesArchiveRejectedEntry
{
    public ArchiveInfo Archive { get; set; }
    public FileProblems[] Files { get; set; }
}
