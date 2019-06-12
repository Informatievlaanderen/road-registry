namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkChangesArchiveFile
    {
        public string File { get; set; }

        public string[] Problems { get; set; }
    }
}
