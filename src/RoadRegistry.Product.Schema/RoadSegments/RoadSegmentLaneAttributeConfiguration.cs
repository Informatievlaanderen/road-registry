namespace RoadRegistry.Product.Schema.RoadSegments;

using BackOffice;
using Extracts.Schemas.ExtractV1.RoadSegments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLaneAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentLaneAttributeRecord>
{
    private const string TableName = "RoadSegmentLaneAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentLaneAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ProductSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}
