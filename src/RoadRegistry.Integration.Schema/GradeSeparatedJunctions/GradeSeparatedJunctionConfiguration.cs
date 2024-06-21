namespace RoadRegistry.Integration.Schema.GradeSeparatedJunctions;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionLatestItem>
{
    private const string TableName = "GradeSeparatedJunction";

    public void Configure(EntityTypeBuilder<GradeSeparatedJunctionLatestItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.UpperRoadSegmentId).IsRequired();
        b.Property(p => p.LowerRoadSegmentId).IsRequired();
    }
}
