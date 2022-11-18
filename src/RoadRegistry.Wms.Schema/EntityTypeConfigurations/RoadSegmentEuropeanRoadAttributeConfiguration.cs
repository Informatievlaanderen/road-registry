namespace RoadRegistry.Wms.Schema.EntityTypeConfigurations;

using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentEuropeanRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentEuropeanRoadAttributeRecord>
{
    public const string TableName = "EuropeseWeg";

    public void Configure(EntityTypeBuilder<RoadSegmentEuropeanRoadAttributeRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.WmsSchema)
            .HasKey(p => p.WS_OIDN)
            .IsClustered(false);

        b.Property(p => p.WS_OIDN)
            .ValueGeneratedNever()
            .IsRequired()
            .HasColumnName("WS_OIDN");

        b.Property(p => p.BEGINORG)
            .HasColumnName("BEGINORG");
        b.Property(p => p.BEGINTIJD)
            .HasColumnName("BEGINTIJD");
        b.Property(p => p.EUNUMMER)
            .HasColumnName("EUNUMMER");
        b.Property(p => p.LBLBGNORG)
            .HasColumnName("LBLBGNORG");
        b.Property(p => p.EU_OIDN)
            .HasColumnName("EU_OIDN");
    }
}
