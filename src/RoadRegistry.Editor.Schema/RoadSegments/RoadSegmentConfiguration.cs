namespace RoadRegistry.Editor.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
{
    private const string TableName = "RoadSegment";

    public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.EditorSchema)
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

        b.HasIndex(p => p.IsRemoved)
            .IsClustered(false);

		//b.OwnsOne(p => p.BoundingBox); // WR-913 Flattened due to performance issues with OwnsOne
        b.Property(p => p.BoundingBoxMaximumX).HasColumnName("BoundingBox_MaximumX");
        b.Property(p => p.BoundingBoxMaximumY).HasColumnName("BoundingBox_MaximumY");
        b.Property(p => p.BoundingBoxMaximumM).HasColumnName("BoundingBox_MaximumM");
        b.Property(p => p.BoundingBoxMinimumX).HasColumnName("BoundingBox_MinimumX");
        b.Property(p => p.BoundingBoxMinimumY).HasColumnName("BoundingBox_MinimumY");
        b.Property(p => p.BoundingBoxMinimumM).HasColumnName("BoundingBox_MinimumM");

        b.HasQueryFilter(p => p.IsRemoved == false);
    }
}
