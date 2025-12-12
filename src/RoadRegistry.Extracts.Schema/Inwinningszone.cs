namespace RoadRegistry.Extracts.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;

public class Inwinningszone
{
    public string NisCode { get; set; }
    public Geometry Contour { get; set; }
    public bool Completed { get; set; }
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
        b.Property(p => p.Completed).IsRequired();

        b.HasIndex(x => x.NisCode);
    }
}
