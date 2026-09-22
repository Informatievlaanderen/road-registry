namespace RoadRegistry.WmsWfsV1Inwinning.Records;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// The road segments and road nodes that are ingewonnen: migrated to V2, or removed as part of that migration.
public class CompletedRoadSegmentRecord
{
    public int WS_OIDN { get; set; }
}

public class CompletedRoadNodeRecord
{
    public int WK_OIDN { get; set; }
}

public class CompletedRoadSegmentRecordConfiguration : IEntityTypeConfiguration<CompletedRoadSegmentRecord>
{
    public void Configure(EntityTypeBuilder<CompletedRoadSegmentRecord> b)
    {
        b.ToTable("CompletedRoadSegments", WellKnownSchemas.WmsWfsV1InwinningSchema).HasKey(p => p.WS_OIDN).IsClustered();
        b.Property(p => p.WS_OIDN).ValueGeneratedNever();
    }
}

public class CompletedRoadNodeRecordConfiguration : IEntityTypeConfiguration<CompletedRoadNodeRecord>
{
    public void Configure(EntityTypeBuilder<CompletedRoadNodeRecord> b)
    {
        b.ToTable("CompletedRoadNodes", WellKnownSchemas.WmsWfsV1InwinningSchema).HasKey(p => p.WK_OIDN).IsClustered();
        b.Property(p => p.WK_OIDN).ValueGeneratedNever();
    }
}
