namespace RoadRegistry.Syndication.Schema;

using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StreetNameConfiguration : IEntityTypeConfiguration<StreetNameRecord>
{
    public void Configure(EntityTypeBuilder<StreetNameRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.SyndicationSchema)
            .HasIndex(p => p.StreetNameId)
            .IsClustered(false);

        b.HasKey(p => p.StreetNameId);

        b.Property(p => p.StreetNameId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.MunicipalityId);
        b.Property(p => p.NisCode);
        b.Property(p => p.Name);
        b.Property(p => p.PersistentLocalId);
        b.Property(p => p.DutchName);
        b.Property(p => p.FrenchName);
        b.Property(p => p.GermanName);
        b.Property(p => p.EnglishName);
        b.Property(p => p.StreetNameStatus);
        b.Property(p => p.HomonymAddition);
        b.Property(p => p.DutchHomonymAddition);
        b.Property(p => p.FrenchHomonymAddition);
        b.Property(p => p.GermanHomonymAddition);
        b.Property(p => p.EnglishHomonymAddition);

        b.HasIndex(p => p.PersistentLocalId);

        b.HasIndex(p => p.Position);

        b.Property(p => p.NameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED");

        b.Property(p => p.DutchNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED");

        b.Property(p => p.FrenchNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED");

        b.Property(p => p.GermanNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED");

        b.Property(p => p.EnglishNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED");
    }

    public const string TableName = "StreetName";
}