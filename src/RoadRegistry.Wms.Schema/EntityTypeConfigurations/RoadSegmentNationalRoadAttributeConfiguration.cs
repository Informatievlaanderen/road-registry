namespace RoadRegistry.Wms.Schema.EntityTypeConfigurations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentNationalRoadAttributeConfiguration : IEntityTypeConfiguration<RoadSegmentNationalRoadAttributeRecord>
{
    public const string TableName = "NationaleWeg";

    public void Configure(EntityTypeBuilder<RoadSegmentNationalRoadAttributeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsSchema)
            .HasKey(p => p.NW_OIDN)
            .IsClustered(false);

        b.Property(p => p.NW_OIDN)
            .ValueGeneratedNever()
            .IsRequired()
            .HasColumnName("NW_OIDN");

        b.Property(p => p.WS_OIDN)
            .HasColumnName("WS_OIDN");
        b.Property(p => p.IDENT2)
            .HasColumnName("IDENT2");
        b.Property(p => p.BEGINORG)
            .HasColumnName("BEGINORG");
        b.Property(p => p.BEGINTIJD)
            .HasColumnName("BEGINTIJD");
        b.Property(p => p.LBLBGNORG)
            .HasColumnName("LBLBGNORG");
    }
}
