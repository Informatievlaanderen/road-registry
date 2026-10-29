namespace RoadRegistry.Pbs.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

// Afgeleide schijnknoop: the road node the afgeleid product needs wherever splitting a road segment on its dynamic
// attributes puts two AfgeleideWegsegmenten end to end. It is not a road node of the registry, so it is kept apart
// from [WegknopenData]: the basisproduct view does not see it, the afgeleid product view adds it to the road nodes.
//
// WS_OIDN is the road segment the split belongs to and VOLGNR its place along that segment (1 is the split nearest
// the begin node), which is how a rebuild of the segment hands the same ids back out. WK_OIDN comes from the
// [AfgeleideSchijnknoopId] sequence, which starts at 1.000.000.000 so it stays clear of the road node ids.
public class DerivedSchijnknoopRecord
{
    public const int InitialId = 1_000_000_000;
    public const string IdSequenceName = "AfgeleideSchijnknoopId";

    public int WK_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public int VOLGNR { get; set; }
    public int? TYPE { get; set; }
    public string? LBLTYPE { get; set; }
    public int? GRENSKNOOP { get; set; }
    public Geometry? GEOMETRIE { get; set; }
    public string CREATIE { get; set; }
    public string VERSIE { get; set; }
}

public class DerivedSchijnknoopRecordConfiguration : IEntityTypeConfiguration<DerivedSchijnknoopRecord>
{
    public const string TableName = "AfgeleideSchijnknopen";

    public void Configure(EntityTypeBuilder<DerivedSchijnknoopRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.WK_OIDN).IsClustered();
        b.Property(p => p.WK_OIDN).ValueGeneratedNever();
        b.Property(p => p.LBLTYPE).HasColumnType("varchar(64)");
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("varchar(15)");
        b.Property(p => p.VERSIE).HasColumnType("varchar(15)");
        b.HasIndex(p => p.WS_OIDN).IsClustered(false);
    }
}
