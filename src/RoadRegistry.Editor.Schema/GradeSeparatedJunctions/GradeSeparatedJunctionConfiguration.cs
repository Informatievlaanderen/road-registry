namespace RoadRegistry.Editor.Schema.GradeSeparatedJunctions;

using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeSeparatedJunctionConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
{
    public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.EditorSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.UpperRoadSegmentId).IsRequired();
        b.Property(p => p.LowerRoadSegmentId).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();
    }

    private const string TableName = "GradeSeparatedJunction";
}
