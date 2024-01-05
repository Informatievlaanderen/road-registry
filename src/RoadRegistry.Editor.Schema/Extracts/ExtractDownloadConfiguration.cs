namespace RoadRegistry.Editor.Schema.Extracts;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractDownloadConfiguration : IEntityTypeConfiguration<ExtractDownloadRecord>
{
    private const string TableName = "ExtractDownload";

    public void Configure(EntityTypeBuilder<ExtractDownloadRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.EditorSchema)
            .HasKey(p => p.DownloadId)
            .IsClustered(false);

        b.Property(p => p.DownloadId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.ArchiveId).IsRequired(false);
        b.Property(p => p.RequestId).IsRequired();
        b.Property(p => p.ExternalRequestId).IsRequired();
        b.Property(p => p.RequestedOn).IsRequired();
        b.Property(p => p.Available).IsRequired();
        b.Property(p => p.AvailableOn).IsRequired();
        b.Property(p => p.IsInformative).IsRequired();
        b.Property(p => p.DownloadedOn).IsRequired(false);

        b.HasIndex(p => p.Available);
    }
}
