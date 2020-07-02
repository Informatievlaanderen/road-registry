namespace RoadRegistry.Wms.Schema
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class RoadSegmentConfiguration : IEntityTypeConfiguration<RoadSegmentRecord>
    {
        public const string TableName = "wegsegmentDenorm";

        public void Configure(EntityTypeBuilder<RoadSegmentRecord> b)
        {
            b.ToTable(TableName, WellknownSchemas.WmsSchema)
                .HasIndex(p => p.Id)
                .IsClustered(false);

            b.Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired()
                .HasColumnName("wegsegmentID");
            b.Property(p => p.RoadSegmentVersion).HasColumnName("wegsegmentversie");
            b.Property(p => p.Geometry2D).HasColumnName("geometrie2D").HasColumnType("Geometry");
            b.Property(p => p.GeometryVersion).HasColumnName("geometrieversie");
            b.Property(p => p.BeginRoadNodeId).HasColumnName("beginWegknoopID");
            b.Property(p => p.EndRoadNodeId).HasColumnName("eindWegknoopID");

            b.Property(p => p.TransactionId).HasColumnName("transactieID");

            b.Property(p => p.AccessRestrictionId).HasColumnName("toegangsbeperking");
            b.Property(p => p.AccessRestrictionDutchName).HasColumnName("lblToegangsbeperking").HasColumnType("varchar(64)");
            b.Property(p => p.MorphologyId).HasColumnName("morfologie");
            b.Property(p => p.MorphologyDutchName).HasColumnName("lblMorfologie").HasColumnType("varchar(64)");
            b.Property(p => p.StatusId).HasColumnName("status");
            b.Property(p => p.StatusDutchName).HasColumnName("lblStatus").HasColumnType("varchar(64)");
            b.Property(p => p.CategoryId).HasColumnName("categorie").HasColumnType("varchar(10)");
            b.Property(p => p.CategoryDutchName).HasColumnName("lblCategorie").HasColumnType("varchar(64)");
            b.Property(p => p.MethodId).HasColumnName("methode");
            b.Property(p => p.MethodDutchName).HasColumnName("lblMethode").HasColumnType("varchar(64)");

            b.Property(p => p.MaintainerId).HasColumnName("beheerder").HasColumnType("varchar(18)");
            b.Property(p => p.MaintainerName).HasColumnName("lblBeheerder").HasColumnType("varchar(64)");

            b.Property(p => p.BeginTime).HasColumnName("begintijd").HasColumnType("varchar(100)");
            b.Property(p => p.BeginOperator).HasColumnName("beginoperator");
            b.Property(p => p.BeginOrganizationId).HasColumnName("beginorganisatie").HasColumnType("varchar(18)");
            b.Property(p => p.BeginOrganizationName).HasColumnName("lblOrganisatie").HasColumnType("varchar(64)");
            b.Property(p => p.BeginApplication).HasColumnName("beginapplicatie").HasColumnType("varchar(100)");

            b.Property(p => p.LeftSideStreetNameId).HasColumnName("linksStraatnaamID");
            b.Property(p => p.RightSideStreetNameId).HasColumnName("rechtsStraatnaamID");
            b.Property(p => p.LeftSideStreetName).HasColumnName("linksStraatnaam").HasColumnType("varchar(128)");
            b.Property(p => p.RightSideStreetName).HasColumnName("rechtsStraatnaam").HasColumnType("varchar(128)");
            b.Property(p => p.LeftSideMunicipalityId).HasColumnName("linksGemeente");
            b.Property(p => p.RightSideMunicipalityId).HasColumnName("rechtsGemeente");

            b.Property(p => p.RecordingDate).HasColumnName("opnamedatum");



        }
    }
}
