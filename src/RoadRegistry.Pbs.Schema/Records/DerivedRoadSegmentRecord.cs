namespace RoadRegistry.Pbs.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

// AfgeleideWegsegment (afgeleid product: the flattened road segment). A road segment is split at every position where
// any dynamic attribute changes, producing potentially multiple rows per WS_OIDN, each with a sub-geometry and the
// attributes resolved to plain (non-dynamic) values. WS_TEMPID is a synthetic identity primary key.
public class DerivedRoadSegmentRecord
{
    public int WS_TEMPID { get; set; }
    public int WS_OIDN { get; set; }
    public int? STATUS { get; set; }
    public string? LBLSTATUS { get; set; }
    public int? METHODE { get; set; }
    public string? LBLMETHODE { get; set; }
    public int? B_WK_OIDN { get; set; }
    public int? E_WK_OIDN { get; set; }
    public int? MORF { get; set; }
    public string? LBLMORF { get; set; }
    public string? WEGCAT { get; set; }
    public string? LBLWEGCAT { get; set; }
    public int? LSTRNMID { get; set; }
    public int? RSTRNMID { get; set; }
    public string? LBEHEER { get; set; }
    public string? RBEHEER { get; set; }
    public int? TOEGANG { get; set; }
    public string? LBLTOEGANG { get; set; }
    public int? VERHARDING { get; set; }
    public string? LBLVERHARD { get; set; }
    public int? AUTOHEEN { get; set; }
    public int? AUTOTERUG { get; set; }
    public int? FIETSHEEN { get; set; }
    public int? FIETSTERUG { get; set; }
    public int? VOETGANGER { get; set; }
    public Geometry? GEOMETRIE { get; set; }
    public string CREATIE { get; set; }
    public string VERSIE { get; set; }
}

public class DerivedRoadSegmentRecordConfiguration : IEntityTypeConfiguration<DerivedRoadSegmentRecord>
{
    public const string TableName = "AfgeleideWegsegmenten";

    public void Configure(EntityTypeBuilder<DerivedRoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.WS_TEMPID).IsClustered();
        b.Property(p => p.WS_TEMPID).ValueGeneratedOnAdd();
        b.Property(p => p.LBLSTATUS).HasColumnType("varchar(64)");
        b.Property(p => p.LBLMETHODE).HasColumnType("varchar(64)");
        b.Property(p => p.LBLMORF).HasColumnType("varchar(64)");
        b.Property(p => p.WEGCAT).HasColumnType("varchar(5)");
        b.Property(p => p.LBLWEGCAT).HasColumnType("varchar(64)");
        b.Property(p => p.LBEHEER).HasColumnType("varchar(18)");
        b.Property(p => p.RBEHEER).HasColumnType("varchar(18)");
        b.Property(p => p.LBLTOEGANG).HasColumnType("varchar(64)");
        b.Property(p => p.LBLVERHARD).HasColumnType("varchar(64)");
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("varchar(15)");
        b.Property(p => p.VERSIE).HasColumnType("varchar(15)");
        b.HasIndex(p => p.WS_OIDN).IsClustered(false);
    }
}
