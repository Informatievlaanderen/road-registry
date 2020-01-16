namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveFile
    {
        public string File { get; set; }

        public RoadNetworkChangesArchiveFileProblem[] Problems { get; set; }
    }
}
