namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

public class RoadNetworkExtractChangesArchiveAcceptedEntry
{
    public ArchiveInfo Archive { get; set; }
    public FileProblems[] Files { get; set; }
}