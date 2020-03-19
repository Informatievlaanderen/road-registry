namespace RoadRegistry.BackOffice.Schema.RoadSegmentNationalRoadAttributes
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentNationalRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNationalRoadAttributeRecord>
    {
        private const string TableName = "RoadSegmentNationalRoadAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentNationalRoadAttributeRecord> b)
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
