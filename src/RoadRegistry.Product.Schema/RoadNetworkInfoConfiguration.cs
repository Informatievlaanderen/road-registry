namespace RoadRegistry.Product.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNetworkInfoConfiguration : IEntityTypeConfiguration<RoadNetworkInfo>
{
    public const string TableName = "RoadNetworkInfo";

    public void Configure(EntityTypeBuilder<RoadNetworkInfo> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ProductSchema)
            .HasIndex(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().HasDefaultValue(RoadNetworkInfo.Identifier).IsRequired();
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
        b.Property(p => p.LastChangedTimestamp).HasDefaultValue(new DateTimeOffset(new DateTime(2025, 3, 28, 10, 24, 52, 812, DateTimeKind.Unspecified).AddTicks(1768), new TimeSpan(0, 0, 0, 0, 0))).IsRequired();
    }
}
