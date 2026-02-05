namespace RoadRegistry.Extracts.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractUpload
{
    public required Guid UploadId { get; set; }
    public required Guid DownloadId { get; set; }
    public required DateTimeOffset UploadedOn { get; set; }
    public required ExtractUploadStatus Status { get; set; }
    public required Guid TicketId { get; set; }
}

public enum ExtractUploadStatus
{
    Processing = 0,
    AutomaticValidationFailed = 1,
    Accepted = 2,
    ManualValidationFailed = 3
}

public class ExtractUploadConfiguration : IEntityTypeConfiguration<ExtractUpload>
{
    private const string TableName = "ExtractUploads";

    public void Configure(EntityTypeBuilder<ExtractUpload> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.UploadId)
            .IsClustered();

        b.Property(p => p.UploadId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.UploadId).IsRequired();
        b.Property(p => p.DownloadId).IsRequired();
        b.Property(p => p.UploadedOn).IsRequired();
        b.Property(p => p.Status).IsRequired();
        b.Property(p => p.TicketId).IsRequired();
    }
}
