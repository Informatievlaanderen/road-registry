namespace RoadRegistry.Integration.Schema.RoadSegments.Version;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentWidthAttributeVersionConfiguration : IEntityTypeConfiguration<RoadSegmentWidthAttributeVersion>
{
    private const string TableName = "road_segment_width_attribute_versions";

    public void Configure(EntityTypeBuilder<RoadSegmentWidthAttributeVersion> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => new { p.Position, p.Id })
            .IsClustered();

        b.Property(p => p.Position).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Position).HasColumnName("position");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.RoadSegmentId).HasColumnName("road_segment_id");
        b.Property(x => x.AsOfGeometryVersion).HasColumnName("as_of_geometry_version");
        b.Property(x => x.Width).HasColumnName("width");
        b.Property(x => x.WidthLabel).HasColumnName("width_label");
        b.Property(x => x.FromPosition).HasColumnName("from_position");
        b.Property(x => x.ToPosition).HasColumnName("to_position");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.OrganizationName).HasColumnName("organization_name");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.VersionAsString).HasColumnName("version_as_string");
        b.Property(RoadSegmentWidthAttributeVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
        b.Property(RoadSegmentWidthAttributeVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

        b.Ignore(x => x.VersionTimestamp);
        b.Ignore(x => x.CreatedOnTimestamp);

        b.HasIndex(p => p.Position);
        b.HasIndex(p => p.Id);
        b.HasIndex(p => p.RoadSegmentId);
        b.HasIndex(p => p.Width);
        b.HasIndex(p => p.WidthLabel);
        b.HasIndex(p => p.OrganizationId);
        b.HasIndex(p => p.OrganizationName);
        b.HasIndex(RoadSegmentWidthAttributeVersion.VersionTimestampBackingPropertyName);
        b.HasIndex(p => p.IsRemoved);
    }
}
