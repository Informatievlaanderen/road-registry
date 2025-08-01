namespace RoadRegistry.Extracts.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        b.Property(p => p.Description).IsRequired(false);
        b.Property(p => p.Contour)
            .HasColumnType("Geometry")
            .IsRequired();

        //b.Property(p => p.ExternalRequestId).IsRequired();
        b.Property(p => p.DownloadId).IsRequired();
        b.Property(p => p.RequestedOn).IsRequired();

        b.Property(p => p.IsInformative).IsRequired();
        //b.Property(p => p.DownloadedOn).IsRequired(false);
        //b.Property(p => p.ArchiveId).IsRequired(false);
        //b.Property(p => p.TicketId).IsRequired(false);

        b.HasIndex(x => x.DownloadId)
            .IsUnique();
    }
}
