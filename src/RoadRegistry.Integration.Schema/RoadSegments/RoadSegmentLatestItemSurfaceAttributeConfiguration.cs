namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLatestItemSurfaceAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentSurfaceAttributeRecord>
{
    private const string TableName = "RoadSegmentSurfaceAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentSurfaceAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}
