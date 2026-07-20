namespace RoadRegistry.WmsWfsV2.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

public class RoadSegmentRecord
{
    public int WS_OIDN { get; set; }
    public int? STATUS { get; set; }
    public string? LBLSTATUS { get; set; }
    public int? METHODE { get; set; }
    public string? LBLMETHODE { get; set; }
    public int? B_WK_OIDN { get; set; }
    public int? E_WK_OIDN { get; set; }
    public Geometry? GEOMETRIE { get; set; }
    public string CREATIE { get; set; }
    public string VERSIE { get; set; }
}

public class RoadSegmentRecordConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
{
    public const string TableName = "Wegsegmenten";

    public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.WS_OIDN).IsClustered();
        b.Property(p => p.WS_OIDN).ValueGeneratedNever();
        b.Property(p => p.LBLSTATUS).HasColumnType("varchar(64)");
        b.Property(p => p.LBLMETHODE).HasColumnType("varchar(64)");
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("varchar(15)");
        b.Property(p => p.VERSIE).HasColumnType("varchar(15)");
    }
}
