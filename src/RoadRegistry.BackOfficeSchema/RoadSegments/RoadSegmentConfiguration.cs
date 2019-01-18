namespace RoadRegistry.BackOfficeSchema.RoadSegments
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
    {
        private const string TableName = "RoadSegment";

        public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasKey(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.ShapeRecordContent);
            b.Property(p => p.ShapeRecordContentLength);
            b.Property(p => p.DbaseRecord);

            b.OwnsOne(p => p.Envelope);
        }
    }
}
