namespace RoadRegistry.WmsWfsV2.Schema.Records;

using System;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

// Ongelijkgrondse kruising (grade separated junction: lower/upper segment + type).
public class GradeSeparatedJunctionRecord
{
    public int OK_OIDN { get; set; }
    public int BO_WS_OIDN { get; set; }
    public int ON_WS_OIDN { get; set; }
    public int? TYPE { get; set; }
    public string? LBLTYPE { get; set; }
    public Geometry? GEOMETRIE { get; set; }
    public DateTimeOffset CREATIE { get; set; }
    public DateTimeOffset VERSIE { get; set; }
}

public class GradeSeparatedJunctionRecordConfiguration : IEntityTypeConfiguration<GradeSeparatedJunctionRecord>
{
    public const string TableName = "OngelijkgrondseKruisingen";

    public void Configure(EntityTypeBuilder<GradeSeparatedJunctionRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsWfsV2Schema).HasKey(p => p.OK_OIDN).IsClustered();
        b.Property(p => p.OK_OIDN).ValueGeneratedNever();
        // Looked up by linked segment id to recompute the intersection point when a segment geometry changes.
        b.HasIndex(p => p.ON_WS_OIDN);
        b.HasIndex(p => p.BO_WS_OIDN);
        b.Property(p => p.LBLTYPE).HasColumnType("varchar(64)");
        b.Property(p => p.GEOMETRIE).HasColumnType("Geometry");
        b.Property(p => p.CREATIE).HasColumnType("datetimeoffset");
        b.Property(p => p.VERSIE).HasColumnType("datetimeoffset");
    }
}
