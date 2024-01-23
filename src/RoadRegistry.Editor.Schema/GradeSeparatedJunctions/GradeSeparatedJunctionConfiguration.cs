namespace RoadRegistry.Editor.Schema.GradeSeparatedJunctions;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
{
    private const string TableName = "GradeSeparatedJunction";

    public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.UpperRoadSegmentId).IsRequired();
        b.Property(p => p.LowerRoadSegmentId).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();
    }
}
