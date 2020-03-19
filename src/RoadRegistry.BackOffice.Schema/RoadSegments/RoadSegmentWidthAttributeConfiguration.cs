namespace RoadRegistry.BackOffice.Schema.RoadSegmentWidthAttributes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentWidthAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentWidthAttributeRecord>
    {
        private const string TableName = "RoadSegmentWidthAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentWidthAttributeRecord> b)
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
