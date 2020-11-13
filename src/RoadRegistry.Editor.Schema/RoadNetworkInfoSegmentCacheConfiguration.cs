namespace RoadRegistry.Editor.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkInfoSegmentCacheConfiguration : IEntityTypeConfiguration<RoadNetworkInfoSegmentCache>
    {
        public const string TableName = "RoadNetworkInfoSegmentCache";

        public void Configure(EntityTypeBuilder<RoadNetworkInfoSegmentCache> b)
        {
            b.ToTable(TableName, WellknownSchemas.EditorSchema)
                .HasIndex(p => p.RoadSegmentId)
                .IsClustered(false);

            b.HasKey(p => p.RoadSegmentId);
            b.Property(p => p.RoadSegmentId).ValueGeneratedNever().IsRequired();
        }
    }
}
