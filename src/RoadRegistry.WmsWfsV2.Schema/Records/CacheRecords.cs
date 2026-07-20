namespace RoadRegistry.WmsWfsV2.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Internal street-name cache (persistent local id -> Dutch name), built from the street-name events, used to fill the
// STRTNM / LSTRNM / RSTRNM label columns. Not part of the exported product.
public class StreetNameCacheRecord
{
    public int StraatnaamId { get; set; }
    public string? Naam { get; set; }
}

public class StreetNameCacheRecordConfiguration : IEntityTypeConfiguration<StreetNameCacheRecord>
{
    public const string TableName = "StraatnaamCache";

    public void Configure(EntityTypeBuilder<StreetNameCacheRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.StraatnaamId);
        b.Property(p => p.StraatnaamId).ValueGeneratedNever();
        b.Property(p => p.Naam).HasColumnType("varchar(255)");
    }
}

// Internal organization cache (organization id -> name + whether the organization is a road maintainer), built from the
// organization events. Holds ALL organizations so a segment's maintainer-id columns can be resolved to a name.
public class OrganizationCacheRecord
{
    public string? OrganisatieId { get; set; }
    public string? Naam { get; set; }
    public string? OvoCode { get; set; }
    public bool IsWegbeheerder { get; set; }
}

public class OrganizationCacheRecordConfiguration : IEntityTypeConfiguration<OrganizationCacheRecord>
{
    public const string TableName = "OrganisatieCache";

    public void Configure(EntityTypeBuilder<OrganizationCacheRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.OrganisatieId);
        b.Property(p => p.OrganisatieId).ValueGeneratedNever().HasColumnType("varchar(18)");
        b.Property(p => p.Naam).HasColumnType("varchar(64)");
        b.Property(p => p.OvoCode).HasColumnType("varchar(9)");
    }
}
