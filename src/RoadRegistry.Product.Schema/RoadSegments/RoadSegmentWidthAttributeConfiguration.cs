namespace RoadRegistry.Product.Schema.RoadSegments;

using Dbase.RoadSegments;
using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentWidthAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentWidthAttributeRecord>
{
    private const string TableName = "RoadSegmentWidthAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentWidthAttributeRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.ProductSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}