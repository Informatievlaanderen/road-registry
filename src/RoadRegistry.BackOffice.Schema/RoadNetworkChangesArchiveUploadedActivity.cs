namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkChangesArchiveUploadedActivity : RoadNetworkActivity
    {
        public string ArchiveId { get; set; }
    }

    public class RoadNetworkChangesArchiveUploadedActivityConfiguration : IEntityTypeConfiguration<RoadNetworkChangesArchiveUploadedActivity>
    {
        public void Configure(EntityTypeBuilder<RoadNetworkChangesArchiveUploadedActivity> b)
        {
            b.Property(p => p.ArchiveId);
        }
    }
}
