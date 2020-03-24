namespace RoadRegistry.BackOffice.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadNetworkInfoConfiguration : IEntityTypeConfiguration<RoadNetworkInfo>
    {
        public const string TableName = "RoadNetworkInfo";

        public void Configure(EntityTypeBuilder<RoadNetworkInfo> b)
        {
            b.ToTable(TableName, WellknownSchemas.BackOfficeSchema)
                .HasIndex(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id).ValueGeneratedNever().HasDefaultValue(0).IsRequired();
            b.Property(p => p.CompletedImport).HasDefaultValue(false).IsRequired();
            b.Property(p => p.OrganizationCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadNodeCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadSegmentCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadSegmentSurfaceAttributeCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadSegmentLaneAttributeCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadSegmentWidthAttributeCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadSegmentEuropeanRoadAttributeCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadSegmentNationalRoadAttributeCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.RoadSegmentNumberedRoadAttributeCount).HasDefaultValue(0).IsRequired();
            b.Property(p => p.GradeSeparatedJunctionCount).HasDefaultValue(0).IsRequired();
        }
    }
}
