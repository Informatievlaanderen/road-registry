namespace RoadRegistry.Syndication.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class StreetNameConfiguration : IEntityTypeConfiguration<StreetNameRecord>
    {
        public const string TableName = "StreetName";

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
        }
    }
}
