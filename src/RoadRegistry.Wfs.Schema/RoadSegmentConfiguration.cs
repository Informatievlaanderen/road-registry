namespace RoadRegistry.Wfs.Schema;

using BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
{
    public const string TableName = "Wegsegment";

    public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
    {
        b.ToTable(TableName, WellKnownSchemas.WfsSchema)
            .HasKey(p => p.Id)
            .IsClustered();

        b.Property(p => p.Id)
            .ValueGeneratedNever()
            .IsRequired()
            .HasColumnName("objectId");

        b.Property(p => p.BeginTime)
            .HasColumnName("versieId")
            .HasColumnType("varchar(100)");

        b.Property(p => p.Geometry2D)
            .HasColumnName("middellijnGeometrie")
            .HasColumnType("Geometry");

        b.Property(p => p.BeginRoadNodeId)
            .HasColumnName("beginknoopObjectId");

        b.Property(p => p.EndRoadNodeId)
            .HasColumnName("eindknoopObjectId");

        b.Property(p => p.StatusDutchName)
            .HasColumnName("wegsegmentstatus")
            .HasColumnType("varchar(64)");

        b.Property(p => p.MorphologyDutchName)
            .HasColumnName("morfologischeWegklasse")
            .HasColumnType("varchar(64)");

        b.Property(p => p.CategoryDutchName)
            .HasColumnName("wegcategorie")
            .HasColumnType("varchar(64)");

        b.Property(p => p.LeftSideStreetNameId)
            .HasColumnName("linkerstraatnaamObjectId");

        b.Property(p => p.LeftSideStreetName)
            .HasColumnName("linkerstraatnaam")
            .HasColumnType("varchar(128)");

        b.Property(p => p.RightSideStreetNameId)
            .HasColumnName("rechterstraatnaamObjectId");

        b.Property(p => p.RightSideStreetName)
            .HasColumnName("rechterstraatnaam")
            .HasColumnType("varchar(128)");

        b.Property(p => p.AccessRestriction)
            .HasColumnName("toegangsbeperking");

        b.Property(p => p.MethodDutchName)
            .HasColumnName("methodeWegsegmentgeometrie")
            .HasColumnType("varchar(64)");

        b.Property(p => p.MaintainerId)
            .HasColumnName("wegbeheerder")
            .HasColumnType("varchar(18)");

        b.Property(p => p.MaintainerName)
            .HasColumnName("labelWegbeheerder")
            .HasColumnType("varchar(64)");

        b.Property(p => p.IsRemoved)
            .HasColumnName("verwijderd")
            .HasDefaultValue(false)
            .IsRequired();
        b.HasIndex(p => p.IsRemoved)
            .IsClustered(false);
    }
}
