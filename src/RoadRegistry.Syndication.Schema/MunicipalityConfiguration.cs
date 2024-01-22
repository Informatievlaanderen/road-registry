namespace RoadRegistry.Syndication.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MunicipalityConfiguration : IEntityTypeConfiguration<MunicipalityRecord>
{
    public const string TableName = "Municipality";

    public void Configure(EntityTypeBuilder<MunicipalityRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.SyndicationSchema)
            .HasIndex(p => p.MunicipalityId)
            .IsClustered(false);

        b.HasKey(p => p.MunicipalityId);

        b.Property(p => p.MunicipalityId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.NisCode);
        b.Property(p => p.DutchName);
        b.Property(p => p.FrenchName);
        b.Property(p => p.GermanName);
        b.Property(p => p.EnglishName);
        b.Property(p => p.MunicipalityStatus);
    }
}
