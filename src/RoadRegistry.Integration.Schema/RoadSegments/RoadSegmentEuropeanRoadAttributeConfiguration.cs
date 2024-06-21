namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentEuropeanRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentEuropeanRoadAttributeLatestItem>
{
    private const string TableName = "RoadSegmentEuropeanRoadAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentEuropeanRoadAttributeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
        b.HasIndex(p => p.IsRemoved);
    }
}
