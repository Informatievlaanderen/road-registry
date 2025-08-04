namespace RoadRegistry.Extracts.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

//TODO-pr add migration
public class ExtractDownload
{
    public Guid DownloadId { get; set; }
    public string ExtractRequestId { get; set; }
    public Geometry Contour { get; set; }
    public bool IsInformative { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool DownloadAvailable { get; set; }
    public bool ExtractDownloadTimeoutOccurred { get; set; }
    public DateTimeOffset? DownloadedOn { get; set; }
    public string? ArchiveId { get; set; }
    public Guid? TicketId { get; set; }
    public bool Closed { get; set; }
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
        b.Property(p => p.DownloadId).IsRequired();
        b.Property(p => p.RequestedOn).IsRequired();
        b.Property(p => p.IsInformative).IsRequired();
        b.Property(p => p.Closed).IsRequired();
        //b.Property(p => p.ExternalRequestId).IsRequired(false);
        //b.Property(p => p.DownloadedOn).IsRequired(false);
        //b.Property(p => p.ArchiveId).IsRequired(false);
        //b.Property(p => p.TicketId).IsRequired(false);

        b.HasIndex(x => x.ExtractRequestId);
    }
}
