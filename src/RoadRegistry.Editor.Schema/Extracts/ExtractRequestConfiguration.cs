namespace RoadRegistry.Editor.Schema.Extracts;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractRequestConfiguration : IEntityTypeConfiguration<ExtractRequestRecord>
{
    private const string TableName = "ExtractRequest";

    public void Configure(EntityTypeBuilder<ExtractRequestRecord> b)
    {
        b.ToTable(TableName, WellknownSchemas.EditorSchema)
            .HasKey(p => p.DownloadId)
            .IsClustered(false);

        b.Property(p => p.DownloadId).ValueGeneratedNever().IsRequired();

        b.Property(p => p.Description).IsRequired();
        b.Property(p => p.Contour).IsRequired();

        b.Property(p => p.RequestId).IsRequired();
        b.Property(p => p.RequestedOn).IsRequired();
        b.Property(p => p.ExternalRequestId).IsRequired();

        b.Property(p => p.Available).IsRequired();
        b.Property(p => p.AvailableOn).IsRequired();

        b.Property(p => p.UploadExpected).IsRequired();
    }
}
