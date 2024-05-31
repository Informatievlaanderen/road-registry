namespace RoadRegistry.Integration.Schema.Extracts;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExtractRequestOverlapConfiguration : IEntityTypeConfiguration<ExtractRequestOverlapRecord>
{
    private const string TableName = "ExtractRequestOverlap";

    public void Configure(EntityTypeBuilder<ExtractRequestOverlapRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.EditorSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        b.Property(p => p.DownloadId1)
            .ValueGeneratedNever()
            .IsRequired();
        b.Property(p => p.DownloadId2)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.Description1).IsRequired(false);
        b.Property(p => p.Description2).IsRequired(false);

        b.Property(p => p.Contour)
            .HasColumnType("Geometry")
            .IsRequired();

        b.HasIndex(p => p.DownloadId1);
        b.HasIndex(p => p.DownloadId2);
    }
}
