namespace RoadRegistry.Sync.MunicipalityRegistry.Models;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

public class Municipality
{
    public string MunicipalityId { get; set; }
    public string NisCode { get; set; }
    public MunicipalityStatus Status { get; set; }
    public Geometry? Geometry { get; set; }
}

public enum MunicipalityStatus
{
    Proposed = 0,
    Current = 1,
    Retired = 2
}

public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public const string TableName = "Municipalities";

    public void Configure(EntityTypeBuilder<Municipality> b)
    {
        b.ToTable(TableName, WellKnownSchemas.MunicipalityEventConsumerSchema)
            .HasKey(p => p.MunicipalityId)
            .IsClustered(false);

        b.Property(p => p.MunicipalityId)
            .ValueGeneratedNever()
            .IsRequired();

        b.Property(p => p.NisCode);
        b.Property(p => p.Status);
        b.Property(p => p.Geometry)
            .HasColumnType("Geometry")
            .IsRequired(false);

        b.HasIndex(p => p.NisCode)
            .IsClustered();
    }
}
