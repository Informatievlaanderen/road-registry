namespace RoadRegistry.WmsWfsV2.Schema.Records;

using System;

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
    public Geometry? GEOMETRIE { get; set; }
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
    // Denormalized street-name labels (resolved from StraatnaamCache at projection time and kept in sync when a street
    // name is renamed/removed). LSTRNM/RSTRNM are the left/right Dutch names; STRNM is the combined display label.
    public string? LSTRNM { get; set; }
    public string? RSTRNM { get; set; }
    public string? STRNM { get; set; }
    public string? LBEHEER { get; set; }
    public string? RBEHEER { get; set; }
    // Denormalized maintainer organization names (left/right) and the maintainer category label, all resolved from the
    // OrganisatieCache at projection time and kept in sync when an organization is renamed/removed. Storing the names
    // lets the WMS view read everything from this table without a join.
    public string? LBLLBEHEER { get; set; }
    public string? LBLRBEHEER { get; set; }
    public string? LBLBEHEER { get; set; }
    public int? TOEGANG { get; set; }
    public string? LBLTOEGANG { get; set; }
    public int? VERHARDING { get; set; }
    public string? LBLVERHARD { get; set; }
    public int? AUTOHEEN { get; set; }
    public int? AUTOTERUG { get; set; }
    public int? FIETSHEEN { get; set; }
    public int? FIETSTERUG { get; set; }
    public int? VOETGANGER { get; set; }
    // Distinct, alphabetically sorted European/National road numbers of the segment, concatenated with " / ".
    public string? EUNUMMERS { get; set; }
    public string? NWNUMMERS { get; set; }
    public DateTimeOffset CREATIE { get; set; }
    public DateTimeOffset VERSIE { get; set; }
}

public class DerivedRoadSegmentRecordConfiguration : IEntityTypeConfiguration<DerivedRoadSegmentRecord>
{
    public const string TableName = "AfgeleideWegsegmenten";

    public void Configure(EntityTypeBuilder<DerivedRoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.WS_TEMPID).IsClustered();
        b.Property(p => p.WS_TEMPID).ValueGeneratedOnAdd();
        b.Property(p => p.LBLSTATUS).HasColumnType("varchar(64)");
        b.Property(p => p.LBLMETHODE).HasColumnType("varchar(64)");
        b.Property(p => p.LBLMORF).HasColumnType("varchar(64)");
        b.Property(p => p.WEGCAT).HasColumnType("varchar(5)");
        b.Property(p => p.LBLWEGCAT).HasColumnType("varchar(64)");
        b.Property(p => p.LBEHEER).HasColumnType("varchar(18)");
        b.Property(p => p.RBEHEER).HasColumnType("varchar(18)");
        b.Property(p => p.LSTRNM).HasColumnType("varchar(255)");
        b.Property(p => p.RSTRNM).HasColumnType("varchar(255)");
        b.Property(p => p.STRNM).HasColumnType("varchar(512)");
        b.Property(p => p.LBLLBEHEER).HasColumnType("varchar(64)");
        b.Property(p => p.LBLRBEHEER).HasColumnType("varchar(64)");
        b.Property(p => p.LBLBEHEER).HasColumnType("varchar(32)");
        b.Property(p => p.LBLTOEGANG).HasColumnType("varchar(64)");
        b.Property(p => p.LBLVERHARD).HasColumnType("varchar(64)");
        b.Property(p => p.EUNUMMERS).HasColumnType("varchar(255)");
        b.Property(p => p.NWNUMMERS).HasColumnType("varchar(255)");
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("datetimeoffset");
        b.Property(p => p.VERSIE).HasColumnType("datetimeoffset");
        b.HasIndex(p => p.WS_OIDN).IsClustered(false);

        // Metadata (coded type) columns — indexed so WMS layers can filter/style the flattened segments by attribute.
        b.HasIndex(p => p.STATUS);
        b.HasIndex(p => p.METHODE);
        b.HasIndex(p => p.MORF);
        b.HasIndex(p => p.WEGCAT);
        b.HasIndex(p => p.TOEGANG);
        b.HasIndex(p => p.VERHARDING);
        b.HasIndex(p => p.AUTOHEEN);
        b.HasIndex(p => p.AUTOTERUG);
        b.HasIndex(p => p.FIETSHEEN);
        b.HasIndex(p => p.FIETSTERUG);
        b.HasIndex(p => p.VOETGANGER);

        // The denormalized street name / maintainer label columns are indexed so WMS layers can filter/style on them.
        b.HasIndex(p => p.STRNM);
        b.HasIndex(p => p.LBLBEHEER);

        // The reference-id/code columns are indexed so a street-name or organization rename can refresh the affected
        // derived rows without scanning the whole table.
        b.HasIndex(p => p.LSTRNMID);
        b.HasIndex(p => p.RSTRNMID);
        b.HasIndex(p => p.LBEHEER);
        b.HasIndex(p => p.RBEHEER);
    }
}
