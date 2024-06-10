namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
{
    private const string TableName = "RoadSegment";

    public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.StartNodeId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.EndNodeId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Geometry).HasColumnType("Geometry").IsRequired();
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.EndNodeId).HasColumnName("end_node_id");
        b.Property(x => x.Geometry).HasColumnName("geometry");
        // public string LastEventHash { get; set; }
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
        b.Property(x => x.MaintainerName).HasColumnName("maintainer_name");
        b.Property(x => x.MethodId).HasColumnName("method_id");
        b.Property(x => x.MethodLabel).HasColumnName("method_label");
        b.Property(x => x.MorphologyId).HasColumnName("morphology_id");
        b.Property(x => x.MorphologyLabel).HasColumnName("morphology_label");
        b.Property(x => x.RightSideStreetNameId).HasColumnName("right_side_street_name_id");
        b.Property(x => x.StatusId).HasColumnName("status_id");
        b.Property(x => x.StatusLabel).HasColumnName("status_label");
        // public int TransactionId { get; set; }
        b.Property(x => x.RecordingDate).HasColumnName("recording_date");
        b.Property(x => x.BeginTime).HasColumnName("begin_time");
        b.Property(x => x.BeginOrganizationId).HasColumnName("begin_organization_id");
        b.Property(x => x.BeginOrganizationName).HasColumnName("begin_organization_name");

        b.HasIndex(p => p.IsRemoved)
            .IsClustered(false);

        b.Property(p => p.BoundingBoxMaximumX).HasColumnName("bounding_box_maximum_x");
        b.Property(p => p.BoundingBoxMaximumY).HasColumnName("bounding_box_maximum_y");
        b.Property(p => p.BoundingBoxMaximumM).HasColumnName("bounding_box_maximum_m");
        b.Property(p => p.BoundingBoxMinimumX).HasColumnName("bounding_box_minimum_x");
        b.Property(p => p.BoundingBoxMinimumY).HasColumnName("bounding_box_minimum_y");
        b.Property(p => p.BoundingBoxMinimumM).HasColumnName("bounding_box_minimum_m");

        b.Property(p => p.Version);
        b.Property(p => p.GeometryVersion);
        b.Property(p => p.AccessRestrictionId);
        b.Property(p => p.MorphologyId);
        b.Property(p => p.StatusId);
        b.Property(p => p.CategoryId);
        b.Property(p => p.MethodId);
        b.Property(p => p.MaintainerId);
        b.Property(p => p.MaintainerName);
        b.Property(p => p.LeftSideStreetNameId);
        b.Property(p => p.RightSideStreetNameId);

        b.Property(p => p.TransactionId);
        b.Property(p => p.RecordingDate);
        b.Property(p => p.BeginTime);
        b.Property(p => p.BeginOrganizationId);
        b.Property(p => p.BeginOrganizationName);

        // Todo: add indexes: MorphologyId, StatusId, CategoryId, MethodId, MaintainerId
        b.HasIndex(p => p.MethodId)
            .IsClustered(false);
        b.HasIndex(p => p.LeftSideStreetNameId)
            .IsClustered(false);
        b.HasIndex(p => p.RightSideStreetNameId)
            .IsClustered(false);
        b.HasIndex(p => p.MaintainerId)
            .IsClustered(false);

        b.HasQueryFilter(p => p.IsRemoved == false);
    }
}
