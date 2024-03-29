namespace RoadRegistry.Product.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
{
    private const string TableName = "RoadSegment";

    public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ProductSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.StartNodeId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.EndNodeId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.ShapeRecordContent).IsRequired();
        b.Property(p => p.ShapeRecordContentLength).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();
        b.Property(p => p.LastEventHash);

        //b.OwnsOne(p => p.BoundingBox); // WR-913 Flattened due to performance issues with OwnsOne
        b.Property(p => p.BoundingBoxMaximumX).HasColumnName("BoundingBox_MaximumX");
        b.Property(p => p.BoundingBoxMaximumY).HasColumnName("BoundingBox_MaximumY");
        b.Property(p => p.BoundingBoxMaximumM).HasColumnName("BoundingBox_MaximumM");
        b.Property(p => p.BoundingBoxMinimumX).HasColumnName("BoundingBox_MinimumX");
        b.Property(p => p.BoundingBoxMinimumY).HasColumnName("BoundingBox_MinimumY");
        b.Property(p => p.BoundingBoxMinimumM).HasColumnName("BoundingBox_MinimumM");

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

        b.HasIndex(p => p.LeftSideStreetNameId)
            .IsClustered(false);
        b.HasIndex(p => p.RightSideStreetNameId)
            .IsClustered(false);
        b.HasIndex(p => p.MaintainerId)
            .IsClustered(false);
    }
}
