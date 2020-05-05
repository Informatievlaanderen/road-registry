namespace RoadRegistry.BackOffice.Schema.RoadSegmentEuropeanRoadAttributes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentEuropeanRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentEuropeanRoadAttributeRecord>
    {
        private const string TableName = "RoadSegmentEuropeanRoadAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentEuropeanRoadAttributeRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.BackOfficeSchema)
                .HasKey(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
            b.Property(p => p.RoadSegmentId).IsRequired();
            b.Property(p => p.DbaseRecord).IsRequired();

            b.HasIndex(p => p.RoadSegmentId);
        }
    }
}
