namespace RoadRegistry.Editor.Schema.RoadNetworkChanges
{
    public class RoadNetworkChangesArchiveAcceptedEntry
    {
        public ArchiveInfo Archive { get; set; }
        public FileProblems[] Files { get; set; }
    }
}
