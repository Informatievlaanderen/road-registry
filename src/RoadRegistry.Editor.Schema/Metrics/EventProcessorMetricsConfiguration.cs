namespace RoadRegistry.Editor.Schema.Metrics;

using BackOffice;
using BackOffice.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EventProcessorMetricsConfiguration : IEntityTypeConfiguration<EventProcessorMetricsRecord>
{
    private const string TableName = "EventProcessors";

    public void Configure(EntityTypeBuilder<EventProcessorMetricsRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.EditorMetricsSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.EventProcessorId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.DbContext).HasDefaultValue(nameof(EditorContext)).IsRequired();
        b.Property(p => p.FromPosition).IsRequired();
        b.Property(p => p.ToPosition).IsRequired();
        b.Property(p => p.ElapsedMilliseconds).IsRequired();
    }
}
