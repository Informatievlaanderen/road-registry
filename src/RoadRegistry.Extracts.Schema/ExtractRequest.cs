namespace RoadRegistry.Extracts.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractRequest
{
    public string ExtractRequestId { get; set; }
    public string OrganizationCode { get; set; }
    public string Description { get; set; }
    public string? ExternalRequestId { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public Guid CurrentDownloadId { get; set; }
}

public class ExtractRequestConfiguration : IEntityTypeConfiguration<ExtractRequest>
{
    private const string TableName = "ExtractRequests";

    public void Configure(EntityTypeBuilder<ExtractRequest> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.ExtractRequestId)
            .IsClustered();

        b.Property(p => p.ExtractRequestId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.Description).IsRequired();
        // b.Property(p => p.Contour)
        //     .HasColumnType("Geometry")
        //     .IsRequired();
        b.Property(p => p.CurrentDownloadId).IsRequired();
        b.Property(p => p.RequestedOn).IsRequired();
        //b.Property(p => p.IsInformative).IsRequired();
        //b.Property(p => p.ExternalRequestId).IsRequired(false);
        //b.Property(p => p.DownloadedOn).IsRequired(false);
        //b.Property(p => p.ArchiveId).IsRequired(false);
        //b.Property(p => p.TicketId).IsRequired(false);

        b.HasIndex(x => x.CurrentDownloadId);
        //     .IsUnique();
    }
}
