namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentDynamicHardeningAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentDynamicHardeningAttributeRecord>
    {
        private const string TableName = "RoadSegmentHardeningAttribute";

        public void Configure(EntityTypeBuilder<RoadSegmentDynamicHardeningAttributeRecord> b)
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
