namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLatestItemNationalRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNationalRoadAttributeLatestItem>
{
    private const string TableName = "RoadSegmentNationalRoadAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentNationalRoadAttributeLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}
