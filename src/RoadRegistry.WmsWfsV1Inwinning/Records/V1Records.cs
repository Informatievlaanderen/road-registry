namespace RoadRegistry.WmsWfsV1Inwinning.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// The V1 WMS and WFS records that are marked IsV2 once what they are about is ingewonnen - no more of them than it takes
// to find them and set IsV2. The tables belong to the Wms and Wfs read models, which migrate them, IsV2 columns and
// indexes included; this context only maps them, so they are excluded from its migrations.
public class V1WmsRoadSegmentRecord
{
    public int Id { get; set; }
    public bool IsV2 { get; set; }
}

public class V1WmsEuropeanRoadRecord
{
    public int EU_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public bool IsV2 { get; set; }
}

public class V1WmsNationalRoadRecord
{
    public int NW_OIDN { get; set; }
    public int WS_OIDN { get; set; }
    public bool IsV2 { get; set; }
}

public class V1WfsRoadSegmentRecord
{
    public int Id { get; set; }
    public bool IsV2 { get; set; }
}

public class V1WfsRoadNodeRecord
{
    public int Id { get; set; }
    public bool IsV2 { get; set; }
}

public class V1WmsRoadSegmentRecordConfiguration : IEntityTypeConfiguration<V1WmsRoadSegmentRecord>
{
    public void Configure(EntityTypeBuilder<V1WmsRoadSegmentRecord> b)
    {
        b.ToTable("wegsegmentDenorm", WellKnownSchemas.WmsDataSchema, t => t.ExcludeFromMigrations()).HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("wegsegmentID").ValueGeneratedNever();
        b.Property(p => p.IsV2).HasColumnName("IsV2");
    }
}

public class V1WmsEuropeanRoadRecordConfiguration : IEntityTypeConfiguration<V1WmsEuropeanRoadRecord>
{
    public void Configure(EntityTypeBuilder<V1WmsEuropeanRoadRecord> b)
    {
        b.ToTable("EuropeseWeg", WellKnownSchemas.WmsDataSchema, t => t.ExcludeFromMigrations()).HasKey(p => p.EU_OIDN);
        b.Property(p => p.EU_OIDN).HasColumnName("EU_OIDN").ValueGeneratedNever();
        b.Property(p => p.WS_OIDN).HasColumnName("WS_OIDN");
        b.Property(p => p.IsV2).HasColumnName("IsV2");
    }
}

public class V1WmsNationalRoadRecordConfiguration : IEntityTypeConfiguration<V1WmsNationalRoadRecord>
{
    public void Configure(EntityTypeBuilder<V1WmsNationalRoadRecord> b)
    {
        b.ToTable("NationaleWeg", WellKnownSchemas.WmsDataSchema, t => t.ExcludeFromMigrations()).HasKey(p => p.NW_OIDN);
        b.Property(p => p.NW_OIDN).HasColumnName("NW_OIDN").ValueGeneratedNever();
        b.Property(p => p.WS_OIDN).HasColumnName("WS_OIDN");
        b.Property(p => p.IsV2).HasColumnName("IsV2");
    }
}

public class V1WfsRoadSegmentRecordConfiguration : IEntityTypeConfiguration<V1WfsRoadSegmentRecord>
{
    public void Configure(EntityTypeBuilder<V1WfsRoadSegmentRecord> b)
    {
        b.ToTable("Wegsegment", WellKnownSchemas.WfsDataSchema, t => t.ExcludeFromMigrations()).HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("objectId").ValueGeneratedNever();
        b.Property(p => p.IsV2).HasColumnName("IsV2");
    }
}

public class V1WfsRoadNodeRecordConfiguration : IEntityTypeConfiguration<V1WfsRoadNodeRecord>
{
    public void Configure(EntityTypeBuilder<V1WfsRoadNodeRecord> b)
    {
        b.ToTable("Wegknoop", WellKnownSchemas.WfsDataSchema, t => t.ExcludeFromMigrations()).HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("objectId").ValueGeneratedNever();
        b.Property(p => p.IsV2).HasColumnName("IsV2");
    }
}
