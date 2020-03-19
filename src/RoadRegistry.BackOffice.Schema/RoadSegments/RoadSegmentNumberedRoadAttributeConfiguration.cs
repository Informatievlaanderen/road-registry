namespace RoadRegistry.BackOffice.Schema.RoadSegmentNumberedRoadAttributes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentNumberedRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNumberedRoadAttributeRecord>
    {
        private const string TableName = "RoadSegmentNumberedRoadAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentNumberedRoadAttributeRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.BackOfficeSchema)
                .HasKey(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.RoadSegmentId);
            b.Property(p => p.DbaseRecord);

            b.HasIndex(p => p.RoadSegmentId);
        }
    }
}
