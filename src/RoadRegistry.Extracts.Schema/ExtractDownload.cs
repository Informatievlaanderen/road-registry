namespace RoadRegistry.Extracts.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

public class ExtractDownload
{
    public Guid DownloadId { get; set; }
    public string ExtractRequestId { get; set; }
    public Geometry Contour { get; set; }
    public bool IsInformative { get; set; }
    public DateTimeOffset RequestedOn { get; set; }

    public ExtractDownloadStatus DownloadStatus { get; set; }
    public DateTimeOffset? DownloadedOn { get; set; }
    public Guid? UploadId { get; set; }
    public DateTimeOffset? UploadedOn { get; set; }
    public ExtractUploadStatus? UploadStatus { get; set; }

    public bool Closed { get; set; }
    public Guid? TicketId { get; set; }
}

public enum ExtractDownloadStatus
{
    Building = 0,
    Error = 1,
    Available = 2
}

public enum ExtractUploadStatus
{
    Processing = 0,
    Rejected = 1,
    Accepted = 2
}

public class ExtractDownloadConfiguration : IEntityTypeConfiguration<ExtractDownload>
{
    private const string TableName = "ExtractDownloads";

    public void Configure(EntityTypeBuilder<ExtractDownload> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.DownloadId)
            .IsClustered();

        b.Property(p => p.DownloadId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.ExtractRequestId).IsRequired();
        b.Property(p => p.Contour)
            .HasColumnType("Geometry")
            .IsRequired();
        b.Property(p => p.IsInformative).IsRequired();
        b.Property(p => p.RequestedOn).IsRequired();
        b.Property(p => p.DownloadStatus).IsRequired();
        b.Property(p => p.DownloadedOn).IsRequired(false);
        b.Property(p => p.UploadId).IsRequired(false);
        b.Property(p => p.UploadedOn).IsRequired(false);
        b.Property(p => p.UploadStatus).IsRequired(false);
        b.Property(p => p.TicketId).IsRequired(false);
        b.Property(p => p.Closed).IsRequired();

        b.HasIndex(x => x.ExtractRequestId);
    }
}
