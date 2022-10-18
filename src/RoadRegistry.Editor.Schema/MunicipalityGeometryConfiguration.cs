namespace RoadRegistry.Editor.Schema;

using Hosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MunicipalityGeometryConfiguration : IEntityTypeConfiguration<MunicipalityGeometry>
{
    public const string TableName = "MunicipalityGeometry";

    public void Configure(EntityTypeBuilder<MunicipalityGeometry> b)
    {
        b.ToTable(TableName, WellknownSchemas.EditorSchema)
            .HasKey(p => p.NisCode)
            .IsClustered();

        b.Property(p => p.NisCode).ValueGeneratedNever().IsRequired().HasMaxLength(5).IsFixedLength();
        b.Property(p => p.Geometry).HasColumnType("Geometry").IsRequired();
    }
}