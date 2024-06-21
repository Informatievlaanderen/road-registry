namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentEuropeanRoadAttributeLatestItemConfiguration : IEntityTypeConfiguration<RoadSegmentEuropeanRoadAttributeLatestItem>
{
    private const string TableName = "road_segment_european_road_attribute_latest_items";

    public void Configure(EntityTypeBuilder<RoadSegmentEuropeanRoadAttributeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.RoadSegmentId).HasColumnName("road_segment_id");
        b.Property(x => x.Number).HasColumnName("number");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.BeginOrganizationId).HasColumnName("begin_organization_id");
        b.Property(x => x.BeginOrganizationName).HasColumnName("begin_organization_name");
        b.Property(x => x.VersionTimestamp).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnTimestamp).HasColumnName("created_on_timestamp");

        b.HasIndex(p => p.RoadSegmentId);
        b.HasIndex(p => p.Number);
        b.HasIndex(p => p.IsRemoved);
    }
}
