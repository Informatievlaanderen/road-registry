namespace RoadRegistry.Sync.StreetNameRegistry.Models;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StreetNameRecord
{
    public string StreetNameId { get; set; }
    public int PersistentLocalId { get; set; }
    public string NisCode { get; set; }
    public string DutchName { get; set; }
    public string FrenchName { get; set; }
    public string GermanName { get; set; }
    public string EnglishName { get; set; }
    public string DutchHomonymAddition { get; set; }
    public string FrenchHomonymAddition { get; set; }
    public string GermanHomonymAddition { get; set; }
    public string EnglishHomonymAddition { get; set; }
    public string DutchNameWithHomonymAddition { get; set; }
    public string FrenchNameWithHomonymAddition { get; set; }
    public string GermanNameWithHomonymAddition { get; set; }
    public string EnglishNameWithHomonymAddition { get; set; }
    public string StreetNameStatus { get; set; }
    public bool IsRemoved { get; set; }
}

public class StreetNameRecordEntityTypeConfiguration : IEntityTypeConfiguration<StreetNameRecord>
{
    private const string TableName = "StreetName";

    public void Configure(EntityTypeBuilder<StreetNameRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.StreetNameSchema)
            .HasIndex(p => p.StreetNameId)
            .IsClustered(false);

        b.HasKey(p => p.StreetNameId);

        b.Property(p => p.StreetNameId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.NisCode);
        b.Property(p => p.PersistentLocalId);
        b.Property(p => p.DutchName).IsRequired(false);
        b.Property(p => p.FrenchName).IsRequired(false);
        b.Property(p => p.GermanName).IsRequired(false);
        b.Property(p => p.EnglishName).IsRequired(false);
        b.Property(p => p.StreetNameStatus).IsRequired(false);
        b.Property(p => p.DutchHomonymAddition).IsRequired(false);
        b.Property(p => p.FrenchHomonymAddition).IsRequired(false);
        b.Property(p => p.GermanHomonymAddition).IsRequired(false);
        b.Property(p => p.EnglishHomonymAddition).IsRequired(false);

        b.HasIndex(p => p.PersistentLocalId);
        b.HasIndex(p => p.StreetNameStatus);

        b.Property(p => p.DutchNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED");

        b.Property(p => p.FrenchNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED");

        b.Property(p => p.GermanNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED");

        b.Property(p => p.EnglishNameWithHomonymAddition)
            .HasComputedColumnSql("COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED");
    }
}
