namespace RoadRegistry.BackOffice.Schema
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkChangesArchiveAcceptedActivity : RoadNetworkActivity
    {
        public string ArchiveId { get; set; }
        public ICollection<RoadNetworkChangesArchiveAcceptedFile> Files { get; set; }
    }

    public class RoadNetworkChangesArchiveAcceptedActivityConfiguration : IEntityTypeConfiguration<RoadNetworkChangesArchiveAcceptedActivity>
    {
        public void Configure(EntityTypeBuilder<RoadNetworkChangesArchiveAcceptedActivity> b)
        {
            b.Property(p => p.ArchiveId);
            b.OwnsMany(p => p.Files, r =>
            {
                r.HasForeignKey("ActivityId");
                r.Property<int>("Id");
                r.HasKey("ActivityId", "Id");
                r.Property(p => p.File);
                r.Ignore(p => p.Problems);
                r.Property(p => p.AllProblems);
            });
        }
    }
}
