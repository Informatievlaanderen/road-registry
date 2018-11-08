namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentSurfaceAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentSurfaceAttributeRecord>
    {
        private const string TableName = "RoadSegmentSurfaceAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentSurfaceAttributeRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasKey(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever();
            b.Property(p => p.RoadSegmentId);
            b.Property(p => p.DbaseRecord);
        }
    }
}
