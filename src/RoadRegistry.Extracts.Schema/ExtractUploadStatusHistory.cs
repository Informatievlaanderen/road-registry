namespace RoadRegistry.Extracts.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractUploadStatusHistory
{
    public int ExtractUploadStatusHistoryId { get; set; }
    public required Guid UploadId { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public required ExtractUploadStatus Status { get; set; }
}

public class ExtractUploadStatusHistoryConfiguration : IEntityTypeConfiguration<ExtractUploadStatusHistory>
{
    private const string TableName = "ExtractUploadStatusHistory";

    public void Configure(EntityTypeBuilder<ExtractUploadStatusHistory> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.ExtractUploadStatusHistoryId)
            .IsClustered();

        b.Property(p => p.ExtractUploadStatusHistoryId)
            .ValueGeneratedOnAdd();

        b.Property(p => p.UploadId).IsRequired();
        b.Property(p => p.Timestamp).IsRequired();
        b.Property(p => p.Status).IsRequired();
    }
}
