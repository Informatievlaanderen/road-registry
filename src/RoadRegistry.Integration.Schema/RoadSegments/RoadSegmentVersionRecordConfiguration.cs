namespace RoadRegistry.Integration.Schema.RoadSegments;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentVersionRecordConfiguration : IEntityTypeConfiguration<RoadSegmentVersionRecord>
{
    private const string TableName = "RoadSegmentVersion";

    public void Configure(EntityTypeBuilder<RoadSegmentVersionRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasKey(x => new { x.StreamId, x.Id, x.Method })
            .IsClustered();

        b.Property(p => p.StreamId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Method).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Version).ValueGeneratedNever().IsRequired();
        b.Property(p => p.GeometryVersion).ValueGeneratedNever().IsRequired();
        b.Property(p => p.RecordingDate).ValueGeneratedNever().IsRequired();
        b.Property(p => p.IsRemoved);

        b.HasIndex(p => p.StreamId)
            .IsClustered(false);
        b.HasIndex(p => p.Id)
            .IsClustered(false);
        b.HasIndex(p => p.Method)
            .IsClustered(false);
        b.HasIndex(p => p.IsRemoved)
            .IsClustered(false);
    }
}
