namespace RoadRegistry.StreetNameConsumer.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StreetNameConsumerItem
{
    public string StreetNameId { get; set; }
    public int? PersistentLocalId { get; set; }
    public string MunicipalityId { get; set; }
    public string NisCode { get; set; }
    public string Name { get; set; }
    public string DutchName { get; set; }
    public string FrenchName { get; set; }
    public string GermanName { get; set; }
    public string EnglishName { get; set; }
    public string HomonymAddition { get; set; }
    public string DutchHomonymAddition { get; set; }
    public string FrenchHomonymAddition { get; set; }
    public string GermanHomonymAddition { get; set; }
    public string EnglishHomonymAddition { get; set; }
    public string NameWithHomonymAddition { get; set; }
    public string DutchNameWithHomonymAddition { get; set; }
    public string FrenchNameWithHomonymAddition { get; set; }
    public string GermanNameWithHomonymAddition { get; set; }
    public string EnglishNameWithHomonymAddition { get; set; }
    public StreetNameStatus? StreetNameStatus { get; set; }
}

public class StreetNameConsumerItemConfiguration : IEntityTypeConfiguration<StreetNameConsumerItem>
{
    public const string TableName = "StreetName";

    public void Configure(EntityTypeBuilder<StreetNameConsumerItem> b)
    {
        b.ToTable(TableName, WellknownSchemas.StreetNameConsumerSchema)
            .HasIndex(p => p.StreetNameId)
            .IsClustered(false);

        b.HasKey(p => p.StreetNameId);

        b.Property(p => p.StreetNameId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.MunicipalityId);
        b.Property(p => p.NisCode);
        b.Property(p => p.Name).IsRequired(false);
        b.Property(p => p.PersistentLocalId);
        b.Property(p => p.DutchName).IsRequired(false);
        b.Property(p => p.FrenchName).IsRequired(false);
        b.Property(p => p.GermanName).IsRequired(false);
        b.Property(p => p.EnglishName).IsRequired(false);
        b.Property(p => p.StreetNameStatus).IsRequired(false);
        b.Property(p => p.HomonymAddition).IsRequired(false);
        b.Property(p => p.DutchHomonymAddition).IsRequired(false);
        b.Property(p => p.FrenchHomonymAddition).IsRequired(false);
        b.Property(p => p.GermanHomonymAddition).IsRequired(false);
        b.Property(p => p.EnglishHomonymAddition).IsRequired(false);

        b.HasIndex(p => p.PersistentLocalId);

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
}
