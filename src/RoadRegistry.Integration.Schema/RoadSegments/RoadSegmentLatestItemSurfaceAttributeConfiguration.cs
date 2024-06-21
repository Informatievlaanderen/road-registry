namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLatestItemSurfaceAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentSurfaceAttributeLatestItem>
{
    private const string TableName = "RoadSegmentSurfaceAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentSurfaceAttributeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}
