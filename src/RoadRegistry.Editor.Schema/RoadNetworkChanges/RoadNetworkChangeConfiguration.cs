namespace RoadRegistry.Editor.Schema.RoadNetworkChanges;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNetworkChangeConfiguration : IEntityTypeConfiguration<RoadNetworkChange>
{
    public const string TableName = "RoadNetworkChange";

    public void Configure(EntityTypeBuilder<RoadNetworkChange> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasIndex(p => p.Id)
            .IsClustered(false);

        b.Property(p => p.Id).ValueGeneratedNever().IsRequired();
        b.Property(p => p.Title).IsRequired();
        b.Property(p => p.Type).IsRequired();
        b.Property(p => p.Content).IsRequired(false);
        b.Property(p => p.When).IsRequired(false);
    }
}
