namespace RoadRegistry.Wms.Schema.EntityTypeConfigurations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OverlappingTransactionZonesRecordConfiguration : IEntityTypeConfiguration<OverlappingTransactionZonesRecord>
{
    private const string TableName = "OverlappendeBijwerkingszones";

    public void Configure(EntityTypeBuilder<OverlappingTransactionZonesRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsSchema)
            .HasKey(p => new { p.DownloadId1, p.DownloadId2 })
            .IsClustered();

        b.Property(p => p.DownloadId1)
            .ValueGeneratedNever()
            .IsRequired();
        b.Property(p => p.DownloadId2)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.Description1)
            .HasColumnName("Omschrijving1")
            .IsRequired(false);
        b.Property(p => p.Description2)
            .HasColumnName("Omschrijving2")
            .IsRequired(false);

        b.Property(p => p.Contour)
            .HasColumnType("Geometry")
            .IsRequired();

        b.HasIndex(p => p.DownloadId1);
        b.HasIndex(p => p.DownloadId2);
    }
}
