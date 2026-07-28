using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class WmsWfsV2LabelsSpatialIndexesTrafficType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // [wms].[Wegsegmenten] is WITH SCHEMABINDING and references the AUTOHEEN/... columns being dropped/renamed, so drop it first.
            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[Wegsegmenten]', N'V') IS NOT NULL
                    DROP VIEW [wms].[Wegsegmenten]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[Wegknopen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[Wegknopen]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[GelijkgrondseKruisingen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[GelijkgrondseKruisingen]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[OngelijkgrondseKruisingen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[OngelijkgrondseKruisingen]; ");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_AUTOHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_AUTOTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropColumn(
                name: "AUTOHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropColumn(
                name: "AUTOTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.RenameColumn(
                name: "VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "VERKEERSTYPE_VOETGANGER");

            migrationBuilder.RenameColumn(
                name: "FIETSTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "VERKEERSTYPE_FIETS");

            migrationBuilder.RenameColumn(
                name: "FIETSHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "VERKEERSTYPE_AUTO");

            migrationBuilder.RenameIndex(
                name: "IX_AfgeleideWegsegmenten_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "IX_AfgeleideWegsegmenten_VERKEERSTYPE_VOETGANGER");

            migrationBuilder.RenameIndex(
                name: "IX_AfgeleideWegsegmenten_FIETSTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "IX_AfgeleideWegsegmenten_VERKEERSTYPE_FIETS");

            migrationBuilder.RenameIndex(
                name: "IX_AfgeleideWegsegmenten_FIETSHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "IX_AfgeleideWegsegmenten_VERKEERSTYPE_AUTO");

            migrationBuilder.AddColumn<string>(
                name: "LBLVERKEERSTYPE_AUTO",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LBLVERKEERSTYPE_FIETS",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LBLVERKEERSTYPE_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wegknopen_LBLTYPE",
                schema: "road",
                table: "Wegknopen",
                column: "LBLTYPE");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLMETHODE",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLMETHODE");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLMORF",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLMORF");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLSTATUS",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLSTATUS");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLTOEGANG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLTOEGANG");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERHARD",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLVERHARD");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERKEERSTYPE_AUTO",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLVERKEERSTYPE_AUTO");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERKEERSTYPE_FIETS",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLVERKEERSTYPE_FIETS");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERKEERSTYPE_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLVERKEERSTYPE_VOETGANGER");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLWEGCAT",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLWEGCAT");

            // Spatial indexes on every geometry column (segments, nodes and both junction kinds). Each table has a
            // single-column clustered primary key, which SQL Server requires for a spatial index.
            CreateSpatialIndex(migrationBuilder, "AfgeleideWegsegmenten");
            CreateSpatialIndex(migrationBuilder, "Wegknopen");
            CreateSpatialIndex(migrationBuilder, "GelijkgrondseKruisingen");
            CreateSpatialIndex(migrationBuilder, "OngelijkgrondseKruisingen");

            // Recreate the view: adds the LBLxxx labels next to their coded columns, the begin/end node ids (B_WK_OIDN,
            // E_WK_OIDN) and the traffic-type columns - both the coded VERKEERSTYPE_* int and the LBLVERKEERSTYPE_* label -
            // which replace the AUTOHEEN/... booleans.
            migrationBuilder.Sql(@"
CREATE VIEW [wms].[Wegsegmenten] WITH SCHEMABINDING AS
SELECT
    ws.[WS_TEMPID] as [TempId]
    ,ws.[GEOMETRIE] as [Geometrie]
    ,ws.[WS_OIDN] as [WegsegmentId]
    ,ws.[B_WK_OIDN] as [Beginknoop]
    ,ws.[E_WK_OIDN] as [Eindknoop]
    ,ws.[METHODE] as [GeometriemethodeId]
    ,ws.[LBLMETHODE] as [Geometriemethode]
    ,ws.[STATUS] as [WegsegmentstatusId]
    ,ws.[LBLSTATUS] as [Wegsegmentstatus]
    ,ws.[MORF] as [MorfologieId]
    ,ws.[LBLMORF] as [Morfologie]
    ,ws.[WEGCAT] as [WegcategorieId]
    ,ws.[LBLWEGCAT] as [Wegcategorie]
    ,ws.[TOEGANG] as [ToegangId]
    ,ws.[LBLTOEGANG] as [Toegang]
    ,ws.[VERHARDING] as [WegverhardingId]
    ,ws.[LBLVERHARD] as [Wegverharding]
    ,ws.[LSTRNMID] as [LinkerstraatnaamId]
    ,ws.[LSTRNM] as [Linkerstraatnaam]
    ,ws.[RSTRNMID] as [RechterstraatnaamId]
    ,ws.[RSTRNM] as [Rechterstraatnaam]
    ,ws.[STRNM] as [StraatnaamLabel]
    ,ws.[LBEHEER] as [LinkerwegbeheerderId]
    ,ws.[LBLLBEHEER] as [Linkerwegbeheerder]
    ,ws.[RBEHEER] as [RechterwegbeheerderId]
    ,ws.[LBLRBEHEER] as [Rechterwegbeheerder]
    ,ws.[LBLBEHEER] as [WegbeheerderLabel]
    ,ws.[VERKEERSTYPE_AUTO] as [VerkeerstypeAutoId]
    ,ws.[LBLVERKEERSTYPE_AUTO] as [VerkeerstypeAuto]
    ,ws.[VERKEERSTYPE_FIETS] as [VerkeerstypeFietsId]
    ,ws.[LBLVERKEERSTYPE_FIETS] as [VerkeerstypeFiets]
    ,ws.[VERKEERSTYPE_VOETGANGER] as [VerkeerstypeVoetgangerId]
    ,ws.[LBLVERKEERSTYPE_VOETGANGER] as [VerkeerstypeVoetganger]
    ,ws.[EUNUMMERS] as [EuNummersLabel]
    ,ws.[NWNUMMERS] as [NwNummersLabel]
    ,ws.[CREATIE] as [Creatie]
    ,ws.[VERSIE] as [Versie]
FROM [road].[AfgeleideWegsegmenten] ws
");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[Wegknopen] WITH SCHEMABINDING AS
SELECT
     [WK_OIDN] as [WegknoopId]
    ,[GEOMETRIE] as [Geometrie]
    ,[TYPE] as [WegknooptypeId]
    ,[LBLTYPE] as [Wegknooptype]
    ,[GRENSKNOOP] as [Grensknoop]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[Wegknopen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[GelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [GK_OIDN] as [GelijkgrondseKruisingId]
    ,[GEOMETRIE] as [Geometrie]
    ,[WS1_OIDN] as [Wegsegment1]
    ,[WS2_OIDN] as [Wegsegment2]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[GelijkgrondseKruisingen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[OngelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [OK_OIDN] as [OngelijkgrondseKruisingId]
    ,[GEOMETRIE] as [Geometrie]
    ,[BO_WS_OIDN] as [BovenliggendWegsegment]
    ,[ON_WS_OIDN] as [OnderliggendWegsegment]
    ,[TYPE] as [OngelijkgrondseKruisingTypeId]
    ,[LBLTYPE] as [OngelijkgrondseKruisingType]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[OngelijkgrondseKruisingen]");
        }

        private static void CreateSpatialIndex(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql($@"
CREATE SPATIAL INDEX [SIDX_{table}_GEOMETRIE] ON [road].[{table}]
(
    [GEOMETRIE]
)USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[Wegsegmenten]', N'V') IS NOT NULL
                    DROP VIEW [wms].[Wegsegmenten]; ");

            migrationBuilder.Sql(@"DROP INDEX [SIDX_AfgeleideWegsegmenten_GEOMETRIE] ON [road].[AfgeleideWegsegmenten];");
            migrationBuilder.Sql(@"DROP INDEX [SIDX_Wegknopen_GEOMETRIE] ON [road].[Wegknopen];");
            migrationBuilder.Sql(@"DROP INDEX [SIDX_GelijkgrondseKruisingen_GEOMETRIE] ON [road].[GelijkgrondseKruisingen];");
            migrationBuilder.Sql(@"DROP INDEX [SIDX_OngelijkgrondseKruisingen_GEOMETRIE] ON [road].[OngelijkgrondseKruisingen];");

            migrationBuilder.DropIndex(
                name: "IX_Wegknopen_LBLTYPE",
                schema: "road",
                table: "Wegknopen");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLMETHODE",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLMORF",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLSTATUS",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLTOEGANG",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERHARD",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERKEERSTYPE_AUTO",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERKEERSTYPE_FIETS",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLVERKEERSTYPE_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLWEGCAT",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropColumn(
                name: "LBLVERKEERSTYPE_AUTO",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropColumn(
                name: "LBLVERKEERSTYPE_FIETS",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropColumn(
                name: "LBLVERKEERSTYPE_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.RenameColumn(
                name: "VERKEERSTYPE_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "VOETGANGER");

            migrationBuilder.RenameColumn(
                name: "VERKEERSTYPE_FIETS",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "FIETSTERUG");

            migrationBuilder.RenameColumn(
                name: "VERKEERSTYPE_AUTO",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "FIETSHEEN");

            migrationBuilder.RenameIndex(
                name: "IX_AfgeleideWegsegmenten_VERKEERSTYPE_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "IX_AfgeleideWegsegmenten_VOETGANGER");

            migrationBuilder.RenameIndex(
                name: "IX_AfgeleideWegsegmenten_VERKEERSTYPE_FIETS",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "IX_AfgeleideWegsegmenten_FIETSTERUG");

            migrationBuilder.RenameIndex(
                name: "IX_AfgeleideWegsegmenten_VERKEERSTYPE_AUTO",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                newName: "IX_AfgeleideWegsegmenten_FIETSHEEN");

            migrationBuilder.AddColumn<int>(
                name: "AUTOHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AUTOTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_AUTOHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "AUTOHEEN");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_AUTOTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "AUTOTERUG");

            // Restore the previous view (AUTOHEEN/... booleans, no LBLxxx/B_WK_OIDN/E_WK_OIDN).
            migrationBuilder.Sql(@"
CREATE VIEW [wms].[Wegsegmenten] WITH SCHEMABINDING AS
SELECT
     ws.[WS_TEMPID]
    ,ws.[GEOMETRIE]
    ,ws.[WS_OIDN]
    ,ws.[METHODE]
    ,ws.[STATUS]
    ,ws.[MORF]
    ,ws.[WEGCAT]
    ,ws.[TOEGANG]
    ,ws.[VERHARDING]
    ,ws.[LSTRNMID]
    ,ws.[RSTRNMID]
    ,ws.[LSTRNM]
    ,ws.[RSTRNM]
    ,ws.[STRNM]
    ,ws.[LBEHEER]
    ,ws.[RBEHEER]
    ,ws.[LBLLBEHEER]
    ,ws.[LBLRBEHEER]
    ,ws.[LBLBEHEER]
    ,ws.[AUTOHEEN]
    ,ws.[AUTOTERUG]
    ,ws.[FIETSHEEN]
    ,ws.[FIETSTERUG]
    ,ws.[VOETGANGER]
    ,ws.[EUNUMMERS]
    ,ws.[NWNUMMERS]
    ,ws.[CREATIE]
    ,ws.[VERSIE]
FROM [road].[AfgeleideWegsegmenten] ws");
        }
    }
}
