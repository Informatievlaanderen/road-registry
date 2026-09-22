using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    /// <inheritdoc />
    public partial class MoveTablesToDataSchemaBehindViewsExcludingV2 : Migration
    {
        // The data tables move to RoadRegistryWmsData and each takes an IsV2 flag, set by the WmsWfsV1Inwinning projection
        // once what a record is about is ingewonnen. A view takes over each table's old name in RoadRegistryWms, with the
        // same columns and without what is V2, so every consumer keeps reading the same name. Bijwerkingszones and
        // OverlappendeBijwerkingszones stay where they are.
        //
        // The views name their columns: a column added to one of these tables later has to be added to its view as well.
        private const string RoadSegmentsView = @"
CREATE VIEW [RoadRegistryWms].[wegsegmentDenorm] AS
SELECT
     [wegsegmentID]
    ,[lblToegangsbeperking]
    ,[toegangsbeperking]
    ,[beginapplicatie]
    ,[beginorganisatie]
    ,[lblOrganisatie]
    ,[beginWegknoopID]
    ,[begintijd]
    ,[lblCategorie]
    ,[categorie]
    ,[eindWegknoopID]
    ,[geometrie2D]
    ,[geometrieversie]
    ,[verwijderd]
    ,[linksGemeente]
    ,[linksGemeenteNisCode]
    ,[linksStraatnaam]
    ,[linksStraatnaamID]
    ,[beheerder]
    ,[lblBeheerder]
    ,[lblMethode]
    ,[methode]
    ,[lblMorfologie]
    ,[morfologie]
    ,[opnamedatum]
    ,[rechtsGemeente]
    ,[rechtsGemeenteNisCode]
    ,[rechtsStraatnaam]
    ,[rechtsStraatnaamID]
    ,[wegsegmentversie]
    ,[lblStatus]
    ,[status]
    ,[transactieID]
FROM [RoadRegistryWmsData].[wegsegmentDenorm]
WHERE [IsV2] = 0";

        private const string EuropeanRoadsView = @"
CREATE VIEW [RoadRegistryWms].[EuropeseWeg] AS
SELECT
     [EU_OIDN]
    ,[BEGINORG]
    ,[BEGINTIJD]
    ,[EUNUMMER]
    ,[LBLBGNORG]
    ,[WS_OIDN]
FROM [RoadRegistryWmsData].[EuropeseWeg]
WHERE [IsV2] = 0";

        private const string NationalRoadsView = @"
CREATE VIEW [RoadRegistryWms].[NationaleWeg] AS
SELECT
     [NW_OIDN]
    ,[BEGINORG]
    ,[BEGINTIJD]
    ,[IDENT2]
    ,[LBLBGNORG]
    ,[WS_OIDN]
FROM [RoadRegistryWmsData].[NationaleWeg]
WHERE [IsV2] = 0";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Schema bound to [wegsegmentDenorm], so it has to go before that table can move.
            migrationBuilder.Sql("DROP VIEW [geolocation].[RoadSegmentGeolocationView];");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryWmsData");

            // Owned by whoever owns RoadRegistryWms, so the tables keep the owner they had: the geolocation view indexed on
            // the road segments requires it, and the views in RoadRegistryWms read these tables through that ownership chain,
            // without grants of their own.
            migrationBuilder.Sql(@"
DECLARE @owner sysname = (SELECT USER_NAME(principal_id) FROM sys.schemas WHERE name = N'RoadRegistryWms');
DECLARE @sql nvarchar(max) = N'ALTER AUTHORIZATION ON SCHEMA::[RoadRegistryWmsData] TO ' + QUOTENAME(@owner);
EXEC(@sql);");

            migrationBuilder.RenameTable(
                name: "wegsegmentDenorm",
                schema: "RoadRegistryWms",
                newName: "wegsegmentDenorm",
                newSchema: "RoadRegistryWmsData");

            migrationBuilder.RenameTable(
                name: "NationaleWeg",
                schema: "RoadRegistryWms",
                newName: "NationaleWeg",
                newSchema: "RoadRegistryWmsData");

            migrationBuilder.RenameTable(
                name: "EuropeseWeg",
                schema: "RoadRegistryWms",
                newName: "EuropeseWeg",
                newSchema: "RoadRegistryWmsData");

            migrationBuilder.AddColumn<bool>(
                name: "IsV2",
                schema: "RoadRegistryWmsData",
                table: "wegsegmentDenorm",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsV2",
                schema: "RoadRegistryWmsData",
                table: "NationaleWeg",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsV2",
                schema: "RoadRegistryWmsData",
                table: "EuropeseWeg",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_IsV2",
                schema: "RoadRegistryWmsData",
                table: "wegsegmentDenorm",
                column: "IsV2")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_NationaleWeg_IsV2",
                schema: "RoadRegistryWmsData",
                table: "NationaleWeg",
                column: "IsV2")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_NationaleWeg_WS_OIDN",
                schema: "RoadRegistryWmsData",
                table: "NationaleWeg",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_EuropeseWeg_IsV2",
                schema: "RoadRegistryWmsData",
                table: "EuropeseWeg",
                column: "IsV2")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_EuropeseWeg_WS_OIDN",
                schema: "RoadRegistryWmsData",
                table: "EuropeseWeg",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            // Unfiltered, as it was.
            CreateGeolocationView(migrationBuilder, "RoadRegistryWmsData");

            migrationBuilder.Sql(RoadSegmentsView);
            migrationBuilder.Sql(EuropeanRoadsView);
            migrationBuilder.Sql(NationalRoadsView);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW [RoadRegistryWms].[wegsegmentDenorm];
DROP VIEW [RoadRegistryWms].[EuropeseWeg];
DROP VIEW [RoadRegistryWms].[NationaleWeg];
DROP VIEW [geolocation].[RoadSegmentGeolocationView];");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_IsV2",
                schema: "RoadRegistryWmsData",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_NationaleWeg_IsV2",
                schema: "RoadRegistryWmsData",
                table: "NationaleWeg");

            migrationBuilder.DropIndex(
                name: "IX_NationaleWeg_WS_OIDN",
                schema: "RoadRegistryWmsData",
                table: "NationaleWeg");

            migrationBuilder.DropIndex(
                name: "IX_EuropeseWeg_IsV2",
                schema: "RoadRegistryWmsData",
                table: "EuropeseWeg");

            migrationBuilder.DropIndex(
                name: "IX_EuropeseWeg_WS_OIDN",
                schema: "RoadRegistryWmsData",
                table: "EuropeseWeg");

            migrationBuilder.DropColumn(
                name: "IsV2",
                schema: "RoadRegistryWmsData",
                table: "wegsegmentDenorm");

            migrationBuilder.DropColumn(
                name: "IsV2",
                schema: "RoadRegistryWmsData",
                table: "NationaleWeg");

            migrationBuilder.DropColumn(
                name: "IsV2",
                schema: "RoadRegistryWmsData",
                table: "EuropeseWeg");

            migrationBuilder.RenameTable(
                name: "wegsegmentDenorm",
                schema: "RoadRegistryWmsData",
                newName: "wegsegmentDenorm",
                newSchema: "RoadRegistryWms");

            migrationBuilder.RenameTable(
                name: "NationaleWeg",
                schema: "RoadRegistryWmsData",
                newName: "NationaleWeg",
                newSchema: "RoadRegistryWms");

            migrationBuilder.RenameTable(
                name: "EuropeseWeg",
                schema: "RoadRegistryWmsData",
                newName: "EuropeseWeg",
                newSchema: "RoadRegistryWms");

            CreateGeolocationView(migrationBuilder, "RoadRegistryWms");
        }

        // As created by AddGeolocationView, on the schema the road segments are in.
        private static void CreateGeolocationView(MigrationBuilder migrationBuilder, string schema)
        {
            migrationBuilder.Sql($@"
CREATE VIEW geolocation.RoadSegmentGeolocationView WITH SCHEMABINDING AS
SELECT
      [geometrie2D].STAsText() AS [GEOMETRYASWKT]
      ,[linksGemeenteNisCode] as [LEFTSIDEMUNICIPALITYNISCODE]
      ,[linksStraatnaam] as [LEFTSIDESTREETNAME]
      ,[linksStraatnaamID] as [LEFTSIDESTREETNAMEID]
      ,[morfologie] as [MORPHOLOGYID]
      ,[rechtsGemeenteNisCode] as [RIGHTSIDEMUNICIPALITYNISCODE]
      ,[rechtsStraatnaam] as [RIGHTSIDESTREETNAME]
      ,[rechtsStraatnaamID] as [RIGHTSIDESTREETNAMEID]
      ,[wegsegmentID] as [Id]
FROM [{schema}].[wegsegmentDenorm]");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_RoadSegmentGeolocationView_ObjectId ON [geolocation].[RoadSegmentGeolocationView] ([Id])");
        }
    }
}
