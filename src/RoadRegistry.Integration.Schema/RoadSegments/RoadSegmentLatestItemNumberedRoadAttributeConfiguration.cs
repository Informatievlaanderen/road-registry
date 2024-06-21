namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLatestItemNumberedRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNumberedRoadAttributeLatestItem>
{
    private const string TableName = "RoadSegmentNumberedRoadAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentNumberedRoadAttributeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}
