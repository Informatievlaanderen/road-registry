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
    public string? QualityReportUrl { get; set; }
}

public enum ExtractUploadStatus
{
    Processing = 0,
    AutomaticValidationFailed = 1,
    Accepted = 2,
    AutomaticValidationSucceeded = 3,
    ManualValidationFailed = 4,

    // The delivery cleared validation - ours and, where it applies, Datavalidatie's - but the road network refused
    // the changes it carries, for instance because they conflict with a neighbouring municipality collected earlier.
    // It is not a validation failure: the delivery was approved and it is Digitaal Vlaanderen who corrects it and has
    // it approved again, which is why the two are not the same status.
    ProcessingFailed = 5
}

public static class ExtractUploadStatusTransitions
{
    // What a rejection by the road network means depends on how far the delivery had come. Before validation
    // succeeded it is the upload that was refused; after, the upload was approved and it is the processing of its
    // changes that failed - the one case where the uploader and Digitaal Vlaanderen are told different things.
    public static ExtractUploadStatus OnRoadNetworkChangesRejected(ExtractUploadStatus currentStatus)
    {
        return currentStatus == ExtractUploadStatus.AutomaticValidationSucceeded
            ? ExtractUploadStatus.ProcessingFailed
            : ExtractUploadStatus.AutomaticValidationFailed;
    }
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
        b.Property(p => p.QualityReportUrl).IsRequired(false);
    }
}
