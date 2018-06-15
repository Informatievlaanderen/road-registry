namespace RoadRegistry.Projections
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using RoadRegistry.Infrastructure;

    public class RoadReferencePointConfiguration : IEntityTypeConfiguration<RoadReferencePointRecord>
    {
        private const string TableName = "RoadReferencePoint";

        public void Configure(EntityTypeBuilder<RoadReferencePointRecord> b)
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
