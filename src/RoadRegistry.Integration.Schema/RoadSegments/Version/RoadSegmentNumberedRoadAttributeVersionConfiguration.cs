namespace RoadRegistry.Integration.Schema.RoadSegments.Version;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentNumberedRoadAttributeVersionConfiguration : IEntityTypeConfiguration<RoadSegmentNumberedRoadAttributeVersion>
{
    private const string TableName = "road_segment_numbered_road_attribute_versions";

    public void Configure(EntityTypeBuilder<RoadSegmentNumberedRoadAttributeVersion> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => new { p.Position, p.Id })
            .IsClustered();

        b.Property(p => p.Position).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Position).HasColumnName("position");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.RoadSegmentId).HasColumnName("road_segment_id");
        b.Property(x => x.Number).HasColumnName("number");
        b.Property(x => x.DirectionId).HasColumnName("direction_id");
        b.Property(x => x.DirectionLabel).HasColumnName("direction_label");
        b.Property(x => x.SequenceNumber).HasColumnName("sequence_number");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.OrganizationName).HasColumnName("organization_name");
        b.Property(x => x.VersionAsString).HasColumnName("version_as_string");
        b.Property(RoadSegmentNumberedRoadAttributeVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
        b.Property(RoadSegmentNumberedRoadAttributeVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

        b.Ignore(x => x.VersionTimestamp);
        b.Ignore(x => x.CreatedOnTimestamp);

        b.HasIndex(p => p.Position);
        b.HasIndex(p => p.Id);
        b.HasIndex(p => p.RoadSegmentId);
        b.HasIndex(p => p.Number);
        b.HasIndex(p => p.DirectionId);
        b.HasIndex(p => p.DirectionLabel);
        b.HasIndex(p => p.OrganizationId);
        b.HasIndex(p => p.OrganizationName);
        b.HasIndex(RoadSegmentNumberedRoadAttributeVersion.VersionTimestampBackingPropertyName);
        b.HasIndex(p => p.IsRemoved);
    }
}
