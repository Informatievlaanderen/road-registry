namespace RoadRegistry.Product.Schema.Metrics;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Metrics;

public class EventProcessorMetricsConfiguration : IEntityTypeConfiguration<EventProcessorMetricsRecord>
{
    private const string TableName = "EventProcessors";

    public void Configure(EntityTypeBuilder<EventProcessorMetricsRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.ProductMetricsSchema)
            .HasKey(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.EventProcessorId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.DbContext).HasDefaultValue(nameof(ProductContext)).IsRequired();
        b.Property(p => p.FromPosition).IsRequired();
        b.Property(p => p.ToPosition).IsRequired();
        b.Property(p => p.ElapsedMilliseconds).IsRequired();
    }
}
