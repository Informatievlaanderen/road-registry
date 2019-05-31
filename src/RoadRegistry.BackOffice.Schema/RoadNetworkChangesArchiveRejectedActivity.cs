namespace RoadRegistry.BackOffice.Schema
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkChangesArchiveRejectedActivity : RoadNetworkActivity
    {
        public string ArchiveId { get; set; }
        public ICollection<RoadNetworkChangesArchiveRejectedFile> Files { get; set; }
    }

    public class RoadNetworkChangesArchiveRejectedActivityConfiguration : IEntityTypeConfiguration<RoadNetworkChangesArchiveRejectedActivity>
    {
        public void Configure(EntityTypeBuilder<RoadNetworkChangesArchiveRejectedActivity> b)
        {
            b.Property(p => p.ArchiveId);
            b.OwnsMany(p => p.Files, r =>
            {
                r.HasForeignKey("ActivityId");
                r.Property<int>("Id");
                r.HasKey("ActivityId", "Id");
                r.Property(p => p.File);
                r.Property(p => p.Problems)
                    .HasConversion(
                        v => string.Join("|", v),
                        v => v.Split('|'));
            });
        }
    }
}
