namespace RoadRegistry.WmsWfsV2.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

// Wegknoop (road node).
public class RoadNodeRecord
{
    public int WK_OIDN { get; set; }
    public int? TYPE { get; set; }
    public string? LBLTYPE { get; set; }
    public int? GRENSKNOOP { get; set; }
    public Geometry? GEOMETRIE { get; set; }
    public string CREATIE { get; set; }
    public string VERSIE { get; set; }
}

public class RoadNodeRecordConfiguration : IEntityTypeConfiguration<RoadNodeRecord>
{
    public const string TableName = "Wegknopen";

    public void Configure(EntityTypeBuilder<RoadNodeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.WK_OIDN).IsClustered();
        b.Property(p => p.WK_OIDN).ValueGeneratedNever();
        b.Property(p => p.LBLTYPE).HasColumnType("varchar(64)");
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("varchar(15)");
        b.Property(p => p.VERSIE).HasColumnType("varchar(15)");
    }
}
