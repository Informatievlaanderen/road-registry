namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLatestItemWidthAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentWidthAttributeLatestItem>
{
    private const string TableName = "RoadSegmentWidthAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentWidthAttributeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}
