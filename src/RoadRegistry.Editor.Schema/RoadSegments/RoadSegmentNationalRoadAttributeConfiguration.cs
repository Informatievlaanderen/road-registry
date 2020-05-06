namespace RoadRegistry.Editor.Schema.RoadSegments
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentNationalRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNationalRoadAttributeRecord>
    {
        private const string TableName = "RoadSegmentNationalRoadAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentNationalRoadAttributeRecord> b)
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
