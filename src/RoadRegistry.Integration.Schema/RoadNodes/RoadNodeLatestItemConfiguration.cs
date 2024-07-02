namespace RoadRegistry.Integration.Schema.RoadNodes;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNodeLatestItemConfiguration : IEntityTypeConfiguration<RoadNodeLatestItem>
{
    private const string TableName = "road_node_latest_items";

    public void Configure(EntityTypeBuilder<RoadNodeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Geometry).HasColumnType("Geometry").IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.TypeId).HasColumnName("type_id");
        b.Property(x => x.TypeLabel).HasColumnName("type_label");
        b.Property(x => x.Version).HasColumnName("version");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.OrganizationName).HasColumnName("organization_name");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.Geometry).HasColumnName("geometry");
        b.Property(x => x.VersionAsString).HasColumnName("version_as_string");
        b.Property(RoadNodeLatestItem.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
        b.Property(RoadNodeLatestItem.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

        b.Property(p => p.BoundingBoxMaximumX).HasColumnName("bounding_box_maximum_x");
        b.Property(p => p.BoundingBoxMaximumY).HasColumnName("bounding_box_maximum_y");
        b.Property(p => p.BoundingBoxMinimumX).HasColumnName("bounding_box_minimum_x");
        b.Property(p => p.BoundingBoxMinimumY).HasColumnName("bounding_box_minimum_y");

        b.Ignore(x => x.VersionTimestamp);
        b.Ignore(x => x.CreatedOnTimestamp);

        b.HasIndex(p => p.TypeId);
        b.HasIndex(p => p.TypeLabel);
        b.HasIndex(p => p.IsRemoved);
        b.HasIndex(p => p.OrganizationId);
        b.HasIndex(p => p.OrganizationName);
        b.HasIndex(RoadNodeLatestItem.VersionTimestampBackingPropertyName);
        b.HasIndex(x => x.Geometry).HasMethod("GIST");
    }
}
