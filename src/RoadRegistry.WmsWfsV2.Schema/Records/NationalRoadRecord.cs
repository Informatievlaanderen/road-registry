namespace RoadRegistry.WmsWfsV2.Schema.Records;

using System;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// Nationale weg (relation table, no geometry).
public class NationalRoadRecord
{
    public int NW_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public string? NWNUMMER { get; set; }
    public DateTimeOffset CREATIE { get; set; }
    public DateTimeOffset VERSIE { get; set; }
}

public class NationalRoadRecordConfiguration : IEntityTypeConfiguration<NationalRoadRecord>
{
    public const string TableName = "NationaleWegen";

    public void Configure(EntityTypeBuilder<NationalRoadRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.NW_OIDN).IsClustered();
        b.Property(p => p.NW_OIDN).ValueGeneratedOnAdd();
        b.Property(p => p.NWNUMMER).HasColumnType("varchar(8)");
        b.Property(p => p.CREATIE).HasColumnType("datetimeoffset");
        b.Property(p => p.VERSIE).HasColumnType("datetimeoffset");
        b.HasIndex(p => p.WS_OIDN).IsClustered(false);
    }
}
