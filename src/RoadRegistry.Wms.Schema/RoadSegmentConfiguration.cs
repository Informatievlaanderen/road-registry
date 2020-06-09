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

            b.Property(p => p.Method).HasColumnName("methode");
            b.Property(p => p.Maintainer).HasColumnName("beheerder").HasColumnType("varchar(18)");
            b.Property(p => p.BeginTime).HasColumnName("begintijd").HasColumnType("varchar(100)");
            b.Property(p => p.BeginOperator).HasColumnName("beginoperator");
            b.Property(p => p.BeginOrganization).HasColumnName("beginorganisatie").HasColumnType("varchar(18)");
            b.Property(p => p.BeginApplication).HasColumnName("beginapplicatie").HasColumnType("varchar(100)");
            b.Property(p => p.Geometry).HasColumnName("geometrie").HasColumnType("Geometry");
            b.Property(p => p.Morphology).HasColumnName("morfologie");
            b.Property(p => p.Status).HasColumnName("status");
            b.Property(p => p.Category).HasColumnName("categorie").HasColumnType("varchar(10)");
            b.Property(p => p.BeginRoadNodeId).HasColumnName("beginWegknoopID");
            b.Property(p => p.EndRoadNodeId).HasColumnName("eindWegknoopID");
            b.Property(p => p.LeftSideStreetNameId).HasColumnName("linksStraatnaamID");
            b.Property(p => p.RightSideStreetNameId).HasColumnName("rechtsStraatnaamID");
            b.Property(p => p.RoadSegmentVersion).HasColumnName("wegsegmentversie");
            b.Property(p => p.GeometryVersion).HasColumnName("geometrieversie");
            b.Property(p => p.RecordingDate).HasColumnName("opnamedatum");
            b.Property(p => p.AccessRestriction).HasColumnName("toegangsbeperking");
            b.Property(p => p.TransactionId).HasColumnName("transactieID");
            b.Property(p => p.SourceId).HasColumnName("sourceID");
            b.Property(p => p.SourceIdSource).HasColumnName("bronSourceID").HasColumnType("varchar(18)");
            b.Property(p => p.LeftSideMunicipality).HasColumnName("linksGemeente");
            b.Property(p => p.RightSideMunicipality).HasColumnName("rechtsGemeente");
            b.Property(p => p.CategoryLabel).HasColumnName("lblCategorie").HasColumnType("varchar(64)");
            b.Property(p => p.MethodLabel).HasColumnName("lblMethode").HasColumnType("varchar(64)");
            b.Property(p => p.MorphologyLabel).HasColumnName("lblMorfologie").HasColumnType("varchar(64)");
            b.Property(p => p.AccessRestrictionLabel).HasColumnName("lblToegangsbeperking").HasColumnType("varchar(64)");
            b.Property(p => p.StatusLabel).HasColumnName("lblStatus").HasColumnType("varchar(64)");
            b.Property(p => p.OrganizationLabel).HasColumnName("lblOrganisatie").HasColumnType("varchar(64)");
            b.Property(p => p.LeftSideStreetNameLabel).HasColumnName("linksStraatnaam").HasColumnType("varchar(128)");
            b.Property(p => p.RightSideStreetNameLabel).HasColumnName("rechtsStraatnaam").HasColumnType("varchar(128)");
            b.Property(p => p.MaintainerLabel).HasColumnName("lblBeheerder").HasColumnType("varchar(64)");
            b.Property(p => p.Geometry2D).HasColumnType("Geometry").HasColumnName("geometrie2D");
        }
    }
}
