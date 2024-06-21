namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLatestItemLaneAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentLaneAttributeLatestItem>
{
    private const string TableName = "road_segment_lane_attribute_latest_items";

    public void Configure(EntityTypeBuilder<RoadSegmentLaneAttributeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();


        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.RoadSegmentId).HasColumnName("road_segment_id");
        b.Property(x => x.Version).HasColumnName("version");
        b.Property(x => x.Count).HasColumnName("count");
        b.Property(x => x.DirectionId).HasColumnName("direction_id");
        b.Property(x => x.DirectionLabel).HasColumnName("direction_label");
        b.Property(x => x.FromPosition).HasColumnName("from_position");
        b.Property(x => x.ToPosition).HasColumnName("to_position");
        b.Property(x => x.BeginOrganizationId).HasColumnName("begin_organization_id");
        b.Property(x => x.BeginOrganizationName).HasColumnName("begin_organization_name");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.VersionTimestamp).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnTimestamp).HasColumnName("created_on_timestamp");

        b.HasIndex(p => p.RoadSegmentId);
        b.HasIndex(p => p.DirectionId);
        b.HasIndex(p => p.DirectionLabel);
        b.HasIndex(p => p.IsRemoved);
    }
}
