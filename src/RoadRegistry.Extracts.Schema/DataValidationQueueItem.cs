namespace RoadRegistry.Extracts.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DataValidationQueueItem
{
    public required Guid UploadId { get; set; }
    public required string SqsRequestJson { get; set; }
    public string? DataValidationId { get; set; }
    public bool Completed { get; set; }

    // When Datavalidatie rejected this delivery and we reported that rejection. A rejected delivery can still be
    // reopened and approved after all, so the queue item stays open; this only keeps us from reporting the same
    // rejection over and over while we keep polling.
    public DateTimeOffset? RejectedOn { get; set; }
}

public class DataValidationQueueItemConfiguration : IEntityTypeConfiguration<DataValidationQueueItem>
{
    private const string TableName = "DataValidationQueue";

    public void Configure(EntityTypeBuilder<DataValidationQueueItem> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.UploadId)
            .IsClustered();

        b.Property(p => p.UploadId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.DataValidationId)
            .IsRequired(false)
            .HasMaxLength(100);
        b.Property(p => p.SqsRequestJson).IsRequired();
        b.Property(p => p.Completed).IsRequired();
        b.Property(p => p.RejectedOn).IsRequired(false);
    }
}
