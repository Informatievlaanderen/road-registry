namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentDynamicWidthAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentDynamicWidthAttributeRecord>
    {
        private const string TableName = "RoadSegmentWidthAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentDynamicWidthAttributeRecord> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasKey(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.RoadSegmentId);
            b.Property(p => p.DbaseRecord);
        }
    }
}
