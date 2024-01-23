namespace RoadRegistry.Wms.Schema.EntityTypeConfigurations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentEuropeanRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentEuropeanRoadAttributeRecord>
{
    public const string TableName = "EuropeseWeg";

    public void Configure(EntityTypeBuilder<RoadSegmentEuropeanRoadAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsSchema)
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
    }
}
