namespace RoadRegistry.Sync.StreetNameRegistry.Models;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RenamedStreetNameRecord
{
    public int StreetNameLocalId { get; set; }
    public int DestinationStreetNameLocalId { get; set; }
}

public class RenamedStreetNameRecordEntityTypeConfiguration : IEntityTypeConfiguration<RenamedStreetNameRecord>
{
    private const string TableName = "RenamedStreetName";

    public void Configure(EntityTypeBuilder<RenamedStreetNameRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.StreetNameEventSchema)
            .HasIndex(p => p.StreetNameLocalId)
            .IsClustered(false);

        b.HasKey(p => p.StreetNameLocalId);

        b.Property(p => p.StreetNameLocalId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.DestinationStreetNameLocalId);
    }
}
