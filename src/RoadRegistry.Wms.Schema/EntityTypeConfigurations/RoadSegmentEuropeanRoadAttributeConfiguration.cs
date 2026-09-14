namespace RoadRegistry.Wms.Schema.EntityTypeConfigurations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentEuropeanRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentEuropeanRoadAttributeRecord>
{
    public const string TableName = "EuropeseWeg";

    public void Configure(EntityTypeBuilder<RoadSegmentEuropeanRoadAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsDataSchema)
            .HasKey(p => p.EU_OIDN)
            .IsClustered(false);

        b.Property(p => p.EU_OIDN)
            .ValueGeneratedNever()
            .IsRequired()
            .HasColumnName("EU_OIDN");

        b.Property(p => p.WS_OIDN)
            .HasColumnName("WS_OIDN");
        b.Property(p => p.BEGINORG)
            .HasColumnName("BEGINORG");
        b.Property(p => p.BEGINTIJD)
            .HasColumnName("BEGINTIJD");
        b.Property(p => p.EUNUMMER)
            .HasColumnName("EUNUMMER");
        b.Property(p => p.LBLBGNORG)
            .HasColumnName("LBLBGNORG");
        b.Property(p => p.IsV2)
            .HasColumnName("IsV2")
            .HasDefaultValue(false)
            .IsRequired();

        b.HasIndex(p => p.IsV2)
            .IsClustered(false);
        // Looked up by road segment to mark the attributes of a road segment that is ingewonnen.
        b.HasIndex(p => p.WS_OIDN)
            .IsClustered(false);
    }
}
