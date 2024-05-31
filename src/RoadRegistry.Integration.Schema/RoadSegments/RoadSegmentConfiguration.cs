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
        b.Property(p => p.ShapeRecordContent).IsRequired();
        b.Property(p => p.ShapeRecordContentLength).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();
        b.Property(p => p.Geometry).HasColumnType("Geometry").IsRequired();
        b.Property(p => p.LastEventHash);
        b.Property(p => p.IsRemoved).HasDefaultValue(false).IsRequired();

        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.EndNodeId).HasColumnName("end_node_id");
        b.Property(x => x.Geometry).HasColumnName("geometry");

    // public int EndNodeId { get; set; }
    // public Geometry Geometry { get; set; }
    // public byte[] ShapeRecordContent { get; set; }
    // public int ShapeRecordContentLength { get; set; }
    // public int StartNodeId { get; set; }
    // public string LastEventHash { get; set; }
    // public bool IsRemoved { get; set; }
    //
    // public int Version { get; set; }
    // public int GeometryVersion { get; set; }
    // public int AccessRestrictionId { get; set; }
    // public string CategoryId { get; set; }
    // public int? LeftSideStreetNameId { get; set; }
    // public string MaintainerId { get; set; }
    // public string MaintainerName { get; set; }
    // public int MethodId { get; set; }
    // public int MorphologyId { get; set; }
    // public int? RightSideStreetNameId { get; set; }
    // public int StatusId { get; set; }
    //
    // public int TransactionId { get; set; }
    // public DateTime RecordingDate { get; set; }
    // public DateTime BeginTime { get; set; }
    // public string BeginOrganizationId { get; set; }
    // public string BeginOrganizationName { get; set; }


        b.HasIndex(p => p.IsRemoved)
            .IsClustered(false);

        //b.OwnsOne(p => p.BoundingBox); // WR-913 Flattened due to performance issues with OwnsOne
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
