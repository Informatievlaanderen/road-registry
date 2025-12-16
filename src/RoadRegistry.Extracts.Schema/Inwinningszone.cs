namespace RoadRegistry.Extracts.Schema;

using System;
using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

public class Inwinningszone
{
    public required string NisCode { get; set; }
    public required Geometry Contour { get; set; }
    public required string Operator { get; set; }
    public required Guid DownloadId { get; set; }
    public required bool Completed { get; set; }
}

public class InwinningszoneConfiguration : IEntityTypeConfiguration<Inwinningszone>
{
    private const string TableName = "Inwinningszones";

    public void Configure(EntityTypeBuilder<Inwinningszone> b)
    {
        b.ToTable(TableName, WellKnownSchemas.ExtractsSchema)
            .HasKey(p => p.NisCode)
            .IsClustered();

        b.Property(p => p.NisCode)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.Contour)
            .HasColumnType("Geometry")
            .IsRequired();
        b.Property(p => p.Operator).IsRequired();
        b.Property(p => p.DownloadId).IsRequired();
        b.Property(p => p.Completed).IsRequired();

        b.HasIndex(x => x.NisCode);
    }
}
