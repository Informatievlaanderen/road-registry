namespace RoadRegistry.Wfs.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadNodeConfiguration : IEntityTypeConfiguration<RoadNodeRecord>
{
    public const string TableName = "Wegknoop";

    public void Configure(EntityTypeBuilder<RoadNodeRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WfsSchema)
            .HasKey(i => i.Id)
            .IsClustered();

        b.Property(p => p.Id)
            .ValueGeneratedNever()
            .IsRequired()
            .HasColumnName("objectId");

        b.Property(p => p.BeginTime)
            .HasColumnName("versieId")
            .HasColumnType("varchar(100)");

        b.Property(p => p.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(255)");

        b.Property(p => p.Geometry)
            .HasColumnName("puntGeometrie")
            .HasColumnType("Geometry");
    }
}
