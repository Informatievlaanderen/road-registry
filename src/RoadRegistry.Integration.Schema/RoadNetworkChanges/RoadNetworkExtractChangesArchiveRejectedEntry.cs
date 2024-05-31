namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

public class RoadNetworkExtractChangesArchiveRejectedEntry
{
    public ArchiveInfo Archive { get; set; }
    public FileProblems[] Files { get; set; }
}