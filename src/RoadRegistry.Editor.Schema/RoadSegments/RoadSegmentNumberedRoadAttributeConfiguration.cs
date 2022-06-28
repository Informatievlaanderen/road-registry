namespace RoadRegistry.Editor.Schema.RoadSegments
{
    using Hosts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentNumberedRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNumberedRoadAttributeRecord>
    {
        private const string TableName = "RoadSegmentNumberedRoadAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentNumberedRoadAttributeRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.EditorSchema)
                .HasKey(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
            b.Property(p => p.RoadSegmentId).IsRequired();
            b.Property(p => p.DbaseRecord).IsRequired();

            b.HasIndex(p => p.RoadSegmentId);
        }
    }
}
