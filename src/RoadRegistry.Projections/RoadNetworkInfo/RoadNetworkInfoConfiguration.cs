namespace RoadRegistry.Projections
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkInfoConfiguration : IEntityTypeConfiguration<RoadNetworkInfo>
    {
        public const string TableName = "RoadNetworkInfo";

        public void Configure(EntityTypeBuilder<RoadNetworkInfo> b)
        {
            b.ToTable(TableName, Schema.Shape)
                .HasIndex(p => p.Id)
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.Property(p => p.CompletedImport).HasDefaultValue(false);
            b.Property(p => p.OrganizationCount).HasDefaultValue(0);
            b.Property(p => p.RoadNodeCount).HasDefaultValue(0);
            b.Property(p => p.RoadSegmentCount).HasDefaultValue(0);
            b.Property(p => p.RoadSegmentDynamicHardeningAttributeCount).HasDefaultValue(0);
            b.Property(p => p.RoadSegmentDynamicLaneAttributeCount).HasDefaultValue(0);
            b.Property(p => p.RoadSegmentDynamicWidthAttributeCount).HasDefaultValue(0);
            b.Property(p => p.RoadSegmentEuropeanRoadAttributeCount).HasDefaultValue(0);
            b.Property(p => p.RoadSegmentNationalRoadAttributeCount).HasDefaultValue(0);
            b.Property(p => p.RoadSegmentNumberedRoadAttributeCount).HasDefaultValue(0);
            b.Property(p => p.ReferencePointCount).HasDefaultValue(0);
            b.Property(p => p.GradeSeparatedJunctionCount).HasDefaultValue(0);
        }
    }
}
