namespace RoadRegistry.BackOffice.Schema.RoadSegments
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
    {
        private const string TableName = "RoadSegment";

        public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.BackOfficeSchema)
                .HasKey(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
            b.Property(p => p.ShapeRecordContent).IsRequired();
            b.Property(p => p.ShapeRecordContentLength).IsRequired();
            b.Property(p => p.DbaseRecord).IsRequired();

            b.OwnsOne(p => p.BoundingBox);
        }
    }
}
