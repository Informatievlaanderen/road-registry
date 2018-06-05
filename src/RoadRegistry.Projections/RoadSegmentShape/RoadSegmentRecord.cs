namespace RoadRegistry.Projections.RoadSegmentShape
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
    {
        private const string TableName = "RoadSegment";

        public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasKey(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.ShapeRecordContent);
            b.Property(p => p.ShapeRecordContentLength);
            b.Property(p => p.DbaseRecord);
        }
    }

}
