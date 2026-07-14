namespace RoadRegistry.Pbs.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Europese weg (relation table, no geometry).
public class EuropeanRoadRecord
{
    public int EU_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public string EUNUMMER { get; set; }
    public string CREATIE { get; set; }
    public string VERSIE { get; set; }
}

public class EuropeanRoadRecordConfiguration : IEntityTypeConfiguration<EuropeanRoadRecord>
{
    public const string TableName = "EuropeseWegen";

    public void Configure(EntityTypeBuilder<EuropeanRoadRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.EU_OIDN).IsClustered();
        b.Property(p => p.EU_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.EUNUMMER).HasColumnType("varchar(4)");
        b.Property(p => p.CREATIE).HasColumnType("varchar(15)");
        b.Property(p => p.VERSIE).HasColumnType("varchar(15)");
        b.HasIndex(p => p.WS_OIDN).IsClustered(false);
    }
}
