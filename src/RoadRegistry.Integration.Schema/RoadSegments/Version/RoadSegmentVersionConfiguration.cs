namespace RoadRegistry.Integration.Schema.RoadSegments.Version;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentVersionConfiguration : IEntityTypeConfiguration<RoadSegmentVersion>
{
    private const string TableName = "road_segment_versions";

    public void Configure(EntityTypeBuilder<RoadSegmentVersion> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => new { p.Position, p.Id })
            .IsClustered();

        b.Property(p => p.Position).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.StartNodeId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.EndNodeId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Geometry).HasColumnType("Geometry").IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Position).HasColumnName("position");
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.EndNodeId).HasColumnName("end_node_id");
        b.Property(x => x.Geometry).HasColumnName("geometry");
        b.Property(x => x.StartNodeId).HasColumnName("start_node_id");
        b.Property(x => x.IsRemoved).HasColumnName("is_removed");
        b.Property(x => x.Version).HasColumnName("version");
        b.Property(x => x.GeometryVersion).HasColumnName("geometry_version");
        b.Property(x => x.AccessRestrictionId).HasColumnName("access_restriction_id");
        b.Property(x => x.AccessRestrictionLabel).HasColumnName("access_restriction_label");
        b.Property(x => x.CategoryId).HasColumnName("category_id");
        b.Property(x => x.CategoryLabel).HasColumnName("category_label");
        b.Property(x => x.LeftSideStreetNameId).HasColumnName("left_side_street_name_id");
        b.Property(x => x.MaintainerId).HasColumnName("maintainer_id");
        b.Property(x => x.MethodId).HasColumnName("method_id");
        b.Property(x => x.MethodLabel).HasColumnName("method_label");
        b.Property(x => x.MorphologyId).HasColumnName("morphology_id");
        b.Property(x => x.MorphologyLabel).HasColumnName("morphology_label");
        b.Property(x => x.RightSideStreetNameId).HasColumnName("right_side_street_name_id");
        b.Property(x => x.StatusId).HasColumnName("status_id");
        b.Property(x => x.StatusLabel).HasColumnName("status_label");
        b.Property(x => x.OrganizationId).HasColumnName("organization_id");
        b.Property(x => x.OrganizationName).HasColumnName("organization_name");
        b.Property(x => x.VersionTimestamp).HasColumnName("version_timestamp");
        b.Property(x => x.CreatedOnTimestamp).HasColumnName("created_on_timestamp");

        b.Property(p => p.BoundingBoxMaximumX).HasColumnName("bounding_box_maximum_x");
        b.Property(p => p.BoundingBoxMaximumY).HasColumnName("bounding_box_maximum_y");
        b.Property(p => p.BoundingBoxMaximumM).HasColumnName("bounding_box_maximum_m");
        b.Property(p => p.BoundingBoxMinimumX).HasColumnName("bounding_box_minimum_x");
        b.Property(p => p.BoundingBoxMinimumY).HasColumnName("bounding_box_minimum_y");
        b.Property(p => p.BoundingBoxMinimumM).HasColumnName("bounding_box_minimum_m");

        b.HasMany(x => x.Lanes)
            .WithOne()
            .IsRequired()
            .HasForeignKey(x => new { x.Position, x.RoadSegmentId });
        b.HasMany(x => x.Surfaces)
            .WithOne()
            .IsRequired()
            .HasForeignKey(x => new { x.Position, x.RoadSegmentId });
        b.HasMany(x => x.Widths)
            .WithOne()
            .IsRequired()
            .HasForeignKey(x => new { x.Position, x.RoadSegmentId });
        b.HasMany(x => x.PartOfEuropeanRoads)
            .WithOne()
            .IsRequired()
            .HasForeignKey(x => new { x.Position, x.RoadSegmentId });
        b.HasMany(x => x.PartOfNationalRoads)
            .WithOne()
            .IsRequired()
            .HasForeignKey(x => new { x.Position, x.RoadSegmentId });
        b.HasMany(x => x.PartOfNumberedRoads)
            .WithOne()
            .IsRequired()
            .HasForeignKey(x => new { x.Position, x.RoadSegmentId });

        b.HasIndex(p => p.Position);
        b.HasIndex(p => p.Id);
        b.HasIndex(p => p.MethodId);
        b.HasIndex(p => p.MethodLabel);
        b.HasIndex(p => p.LeftSideStreetNameId);
        b.HasIndex(p => p.RightSideStreetNameId);
        b.HasIndex(p => p.MaintainerId);
        b.HasIndex(p => p.MorphologyId);
        b.HasIndex(p => p.MorphologyLabel);
        b.HasIndex(p => p.CategoryId);
        b.HasIndex(p => p.CategoryLabel);
        b.HasIndex(p => p.StatusId);
        b.HasIndex(p => p.StatusLabel);
        b.HasIndex(p => p.IsRemoved);
        b.HasIndex(x => new { x.IsRemoved, x.StatusId });
        b.HasIndex(p => p.StartNodeId);
        b.HasIndex(p => p.EndNodeId);
        b.HasIndex(p => p.OrganizationId);
        b.HasIndex(p => p.OrganizationName);
        b.HasIndex(p => p.VersionTimestamp);
        b.HasIndex(x => x.Geometry).HasMethod("GIST");
    }
}
