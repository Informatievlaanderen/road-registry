namespace RoadRegistry.BackOffice.Schema.RoadSegmentLaneAttributes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentLaneAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentLaneAttributeRecord>
    {
        private const string TableName = "RoadSegmentLaneAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentLaneAttributeRecord> b)
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
