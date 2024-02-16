namespace RoadRegistry.Sync.StreetNameRegistry.TypeConfigurations
{
    using BackOffice;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RenamedStreetNameRecordEntityTypeConfiguration : IEntityTypeConfiguration<RenamedStreetNameRecord>
    {
        public const string TableName = "RenamedStreetName";

        public void Configure(EntityTypeBuilder<RenamedStreetNameRecord> b)
        {
            b.ToTable(TableName, WellKnownSchemas.StreetNameSchema)
                .HasIndex(p => p.StreetNameLocalId)
                .IsClustered(false);

            b.HasKey(p => p.StreetNameLocalId);

            b.Property(p => p.StreetNameLocalId)
                .ValueGeneratedNever()
                .IsRequired();

            b.Property(p => p.DestinationStreetNameLocalId);
        }
    }
}
