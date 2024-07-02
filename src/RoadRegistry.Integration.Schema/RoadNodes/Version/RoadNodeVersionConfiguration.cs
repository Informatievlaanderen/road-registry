namespace RoadRegistry.Integration.Schema.RoadNodes.Version;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNodeVersionConfiguration : IEntityTypeConfiguration<RoadNodeVersion>
{
    private const string TableName = "road_node_versions";

    public void Configure(EntityTypeBuilder<RoadNodeVersion> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => new { p.Position, p.Id })
            .IsClustered();

        b.Property(p => p.Position).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Geometry).HasColumnType("Geometry").IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(p => p.Position).HasColumnName("position");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.TypeId).HasColumnName("type_id");
        b.Property(x => x.TypeLabel).HasColumnName("type_label");
        b.Property(x => x.Version).HasColumnName("version");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.OrganizationName).HasColumnName("organization_name");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.Geometry).HasColumnName("geometry");
        b.Property(x => x.VersionAsString).HasColumnName("version_as_string");
        b.Property(RoadNodeVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
        b.Property(RoadNodeVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

        b.Property(p => p.BoundingBoxMaximumX).HasColumnName("bounding_box_maximum_x");
        b.Property(p => p.BoundingBoxMaximumY).HasColumnName("bounding_box_maximum_y");
        b.Property(p => p.BoundingBoxMinimumX).HasColumnName("bounding_box_minimum_x");
        b.Property(p => p.BoundingBoxMinimumY).HasColumnName("bounding_box_minimum_y");

        b.Ignore(x => x.VersionTimestamp);
        b.Ignore(x => x.CreatedOnTimestamp);

        b.HasIndex(p => p.Position);
        b.HasIndex(p => p.Id);
        b.HasIndex(p => p.TypeId);
        b.HasIndex(p => p.TypeLabel);
        b.HasIndex(p => p.IsRemoved);
        b.HasIndex(p => p.OrganizationId);
        b.HasIndex(p => p.OrganizationName);
        b.HasIndex(RoadNodeVersion.VersionTimestampBackingPropertyName);
        b.HasIndex(x => x.Geometry).HasMethod("GIST");
    }
}
