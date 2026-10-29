namespace RoadRegistry.Pbs.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

// Wegknoop (road node). The projection writes one row per road node into [WegknopenData]; the two download products
// read it through the [Wegknopen] (basisproduct) and [AfgeleideWegknopen] (afgeleid product) views, which today
// select the same rows and the same columns but are about to diverge: the basisproduct keeps only the v1 schijnknopen,
// the afgeleid product carries the v2 ones the dynamic-attribute split adds as well (GAWR-7836).
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
    public const string TableName = "WegknopenData";

    public void Configure(EntityTypeBuilder<RoadNodeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.PbsSchema).HasKey(p => p.WK_OIDN).IsClustered();
        b.Property(p => p.WK_OIDN).ValueGeneratedNever();
        b.Property(p => p.LBLTYPE).HasColumnType("varchar(64)");
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("varchar(15)");
        b.Property(p => p.VERSIE).HasColumnType("varchar(15)");
    }
}
