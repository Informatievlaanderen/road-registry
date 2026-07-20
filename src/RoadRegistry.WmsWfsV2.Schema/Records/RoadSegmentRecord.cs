namespace RoadRegistry.WmsWfsV2.Schema.Records;

using System;

using System.Text.Json;
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

    // The persisted column: the segment's position-varying attributes serialized as JSON. Internal staging only — it lets
    // the projection re-flatten AfgeleideWegsegmenten after a partial change without a table per attribute; no view reads it.
    public string? DynamicAttributesJson { get; set; }

    public DateTimeOffset CREATIE { get; set; }
    public DateTimeOffset VERSIE { get; set; }

    // Convenience accessor over DynamicAttributesJson (not mapped). Getting deserializes; setting serializes, so assigning
    // it marks the underlying column as changed.
    public RoadSegmentDynamicAttributes DynamicAttributes
    {
        get => string.IsNullOrEmpty(DynamicAttributesJson)
            ? new RoadSegmentDynamicAttributes()
            : JsonSerializer.Deserialize<RoadSegmentDynamicAttributes>(DynamicAttributesJson) ?? new RoadSegmentDynamicAttributes();
        set => DynamicAttributesJson = JsonSerializer.Serialize(value);
    }
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
        b.Property(p => p.DynamicAttributesJson).HasColumnName("DynamicAttributes").HasColumnType("nvarchar(max)");
        b.Property(p => p.CREATIE).HasColumnType("datetimeoffset");
        b.Property(p => p.VERSIE).HasColumnType("datetimeoffset");
        b.Ignore(p => p.DynamicAttributes);
    }
}
