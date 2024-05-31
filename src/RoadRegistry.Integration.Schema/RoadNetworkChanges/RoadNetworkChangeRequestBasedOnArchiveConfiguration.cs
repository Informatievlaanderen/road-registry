namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNetworkChangeRequestBasedOnArchiveConfiguration : IEntityTypeConfiguration<RoadNetworkChangeRequestBasedOnArchive>
{
    public const string TableName = "RoadNetworkChangeRequestBasedOnArchive";

    public void Configure(EntityTypeBuilder<RoadNetworkChangeRequestBasedOnArchive> b)
    {
        b.ToTable(TableName, WellKnownSchemas.IntegrationSchema)
            .HasIndex(p => p.ChangeRequestId)
            .IsClustered(false);

        b.HasKey(p => p.ChangeRequestId);
        b.Property(p => p.ChangeRequestId).ValueGeneratedNever().IsRequired().HasMaxLength(32);
        b.Property(p => p.ArchiveId).IsRequired();
    }
}
