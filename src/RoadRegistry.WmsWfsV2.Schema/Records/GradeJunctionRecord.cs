namespace RoadRegistry.WmsWfsV2.Schema.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

// Gelijkgrondse kruising (grade junction: two segments crossing at grade).
public class GradeJunctionRecord
{
    public int GK_OIDN { get; set; }
    public int WS1_OIDN { get; set; }
    public int WS2_OIDN { get; set; }
    public Geometry? GEOMETRIE { get; set; }
    public string CREATIE { get; set; }
    public string VERSIE { get; set; }
}

public class GradeJunctionRecordConfiguration : IEntityTypeConfiguration<GradeJunctionRecord>
{
    public const string TableName = "GelijkgrondseKruisingen";

    public void Configure(EntityTypeBuilder<GradeJunctionRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.GK_OIDN).IsClustered();
        b.Property(p => p.GK_OIDN).ValueGeneratedNever();
        // Looked up by linked segment id to recompute the intersection point when a segment geometry changes.
        b.HasIndex(p => p.WS1_OIDN);
        b.HasIndex(p => p.WS2_OIDN);
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("varchar(15)");
        b.Property(p => p.VERSIE).HasColumnType("varchar(15)");
    }
}
