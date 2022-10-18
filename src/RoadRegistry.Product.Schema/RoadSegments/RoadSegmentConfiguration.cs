namespace RoadRegistry.Product.Schema.RoadSegments;

using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
{
    public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.ProductSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.ShapeRecordContent).IsRequired();
        b.Property(p => p.ShapeRecordContentLength).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();

        b.OwnsOne(p => p.BoundingBox);
    }

    private const string TableName = "RoadSegment";
}
