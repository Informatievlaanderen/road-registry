namespace RoadRegistry.Editor.Schema.Extracts;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractUploadConfiguration : IEntityTypeConfiguration<ExtractUploadRecord>
{
    private const string TableName = "ExtractUpload";

    public void Configure(EntityTypeBuilder<ExtractUploadRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasKey(p => p.UploadId)
            .IsClustered(false);

        b.Property(p => p.UploadId).ValueGeneratedNever().IsRequired();
        b.Property(p => p.DownloadId).IsRequired();
        b.Property(p => p.ArchiveId).IsRequired();
        b.Property(p => p.RequestId).IsRequired();
        b.Property(p => p.ExternalRequestId).IsRequired();
        b.Property(p => p.ChangeRequestId).IsRequired();
        b.Property(p => p.ReceivedOn).IsRequired();
        b.Property(p => p.Status).IsRequired();
        b.Property(p => p.CompletedOn).IsRequired();

        b.HasIndex(p => p.ChangeRequestId).IsUnique();
    }
}
