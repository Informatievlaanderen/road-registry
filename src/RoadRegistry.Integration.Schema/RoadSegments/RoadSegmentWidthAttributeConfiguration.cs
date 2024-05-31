namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentWidthAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentWidthAttributeRecord>
{
    private const string TableName = "RoadSegmentWidthAttribute";

    public void Configure(EntityTypeBuilder<RoadSegmentWidthAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }
}
