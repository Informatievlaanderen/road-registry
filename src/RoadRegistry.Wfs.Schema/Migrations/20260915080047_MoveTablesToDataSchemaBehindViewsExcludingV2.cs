using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class MoveTablesToDataSchemaBehindViewsExcludingV2 : Migration
    {
        // The data tables move to RoadRegistryWfsData and each takes an IsV2 flag, set by the WmsWfsV1Inwinning projection
        // once the road segment or road node is ingewonnen. A view takes over each table's old name in RoadRegistryWfs,
        // with the same columns and without what is V2, so every consumer keeps reading the same name.
        //
        // The views name their columns: a column added to one of these tables later has to be added to its view as well.
        private const string RoadSegmentsView = @"
CREATE VIEW [RoadRegistryWfs].[Wegsegment] AS
SELECT
     [objectId]
    ,[toegangsbeperking]
    ,[beginknoopObjectId]
    ,[versieId]
    ,[wegcategorie]
    ,[eindknoopObjectId]
    ,[middellijnGeometrie]
    ,[verwijderd]
    ,[linkerstraatnaam]
    ,[linkerstraatnaamObjectId]
    ,[wegbeheerder]
    ,[labelWegbeheerder]
    ,[methodeWegsegmentgeometrie]
    ,[morfologischeWegklasse]
    ,[rechterstraatnaam]
    ,[rechterstraatnaamObjectId]
    ,[wegsegmentstatus]
FROM [RoadRegistryWfsData].[Wegsegment]
WHERE [IsV2] = 0";

        private const string RoadNodesView = @"
CREATE VIEW [RoadRegistryWfs].[Wegknoop] AS
SELECT
     [objectId]
    ,[versieId]
    ,[puntGeometrie]
    ,[type]
FROM [RoadRegistryWfsData].[Wegknoop]
WHERE [IsV2] = 0";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryWfsData");

            // Owned by whoever owns RoadRegistryWfs, so the tables keep the owner they had: the views in RoadRegistryWfs
            // read them through that ownership chain, without grants of their own.
            migrationBuilder.Sql(@"
DECLARE @owner sysname = (SELECT USER_NAME(principal_id) FROM sys.schemas WHERE name = N'RoadRegistryWfs');
DECLARE @sql nvarchar(max) = N'ALTER AUTHORIZATION ON SCHEMA::[RoadRegistryWfsData] TO ' + QUOTENAME(@owner);
EXEC(@sql);");

            migrationBuilder.RenameTable(
                name: "Wegsegment",
                schema: "RoadRegistryWfs",
                newName: "Wegsegment",
                newSchema: "RoadRegistryWfsData");

            migrationBuilder.RenameTable(
                name: "Wegknoop",
                schema: "RoadRegistryWfs",
                newName: "Wegknoop",
                newSchema: "RoadRegistryWfsData");

            migrationBuilder.AddColumn<bool>(
                name: "IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegsegment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegknoop",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegsegment",
                column: "IsV2")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegknoop_IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegknoop",
                column: "IsV2")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.Sql(RoadSegmentsView);
            migrationBuilder.Sql(RoadNodesView);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW [RoadRegistryWfs].[Wegsegment];
DROP VIEW [RoadRegistryWfs].[Wegknoop];");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegknoop_IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegknoop");

            migrationBuilder.DropColumn(
                name: "IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegsegment");

            migrationBuilder.DropColumn(
                name: "IsV2",
                schema: "RoadRegistryWfsData",
                table: "Wegknoop");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryWfs");

            migrationBuilder.RenameTable(
                name: "Wegsegment",
                schema: "RoadRegistryWfsData",
                newName: "Wegsegment",
                newSchema: "RoadRegistryWfs");

            migrationBuilder.RenameTable(
                name: "Wegknoop",
                schema: "RoadRegistryWfsData",
                newName: "Wegknoop",
                newSchema: "RoadRegistryWfs");
        }
    }
}
