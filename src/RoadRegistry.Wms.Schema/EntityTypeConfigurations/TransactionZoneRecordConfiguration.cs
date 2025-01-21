namespace RoadRegistry.Wms.Schema.EntityTypeConfigurations;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionZoneRecordConfiguration : IEntityTypeConfiguration<TransactionZoneRecord>
{
    public const string TableName = "Bijwerkingszones";

    public void Configure(EntityTypeBuilder<TransactionZoneRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WmsSchema)
            .HasKey(p => p.DownloadId)
            .IsClustered();

        b.Property(p => p.DownloadId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.Description)
            .HasColumnName("Omschrijving")
            .IsRequired(false);

        b.Property(p => p.Contour)
            .HasColumnType("Geometry")
            .IsRequired();
    }
}
