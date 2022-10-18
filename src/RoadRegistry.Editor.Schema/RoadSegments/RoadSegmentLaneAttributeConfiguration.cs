namespace RoadRegistry.Editor.Schema.RoadSegments;

using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentLaneAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentLaneAttributeRecord>
{
    public void Configure(EntityTypeBuilder<RoadSegmentLaneAttributeRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.EditorSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RoadSegmentId).IsRequired();
        b.Property(p => p.DbaseRecord).IsRequired();

        b.HasIndex(p => p.RoadSegmentId);
    }

    private const string TableName = "RoadSegmentLaneAttribute";
}