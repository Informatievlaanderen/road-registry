namespace RoadRegistry.Editor.Schema.RoadNodes;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNodeConfiguration : IEntityTypeConfiguration<RoadNodeRecord>
{
    private const string TableName = "RoadNode";

    public void Configure(EntityTypeBuilder<RoadNodeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.ShapeRecordContent).IsRequired();
        b.Property(p => p.ShapeRecordContentLength).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();
        b.Property(p => p.Geometry).HasColumnType("Geometry").IsRequired();

		//b.OwnsOne(p => p.BoundingBox); // WR-913 Flattened due to performance issues with OwnsOne
        b.Property(p => p.BoundingBoxMaximumX).HasColumnName("BoundingBox_MaximumX");
        b.Property(p => p.BoundingBoxMaximumY).HasColumnName("BoundingBox_MaximumY");
        b.Property(p => p.BoundingBoxMinimumX).HasColumnName("BoundingBox_MinimumX");
        b.Property(p => p.BoundingBoxMinimumY).HasColumnName("BoundingBox_MinimumY");
    }
}
