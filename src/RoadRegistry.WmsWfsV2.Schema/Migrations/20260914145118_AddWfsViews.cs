using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddWfsViews : Migration
    {
        // The WFS views expose the same four entities as their WMS counterparts, but only what identifies a feature and
        // places it in time: the object id, its persistent URI (the OSLO namespace followed by the object id), the
        // geometry, and the creation and version timestamps.
        //
        // A WFS feature has to be unique by its identifier, so [wfs].[Wegsegmenten] reads the road segments themselves
        // rather than the flattened [AfgeleideWegsegmenten] the WMS view renders, which hold a row per attribute section.
        // Rows without a geometry cannot be served as a feature, so - as in the WMS view - a grade separated junction
        // is only exposed once its geometry is known.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'wfs')
                    EXEC(N'CREATE SCHEMA [wfs]');");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[Wegsegmenten] WITH SCHEMABINDING AS
SELECT
     [WS_OIDN] as [ObjectId]
    ,CONCAT('https://data.vlaanderen.be/id/wegsegment/', [WS_OIDN]) as [Id]
    ,[GEOMETRIE] as [Lijngeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[Wegsegmenten]");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[Wegknopen] WITH SCHEMABINDING AS
SELECT
     [WK_OIDN] as [ObjectId]
    ,CONCAT('https://data.vlaanderen.be/id/wegknoop/', [WK_OIDN]) as [Id]
    ,[GEOMETRIE] as [Puntgeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[Wegknopen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[GelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [GK_OIDN] as [ObjectId]
    ,CONCAT('https://data.vlaanderen.be/id/gelijkgrondsekruising/', [GK_OIDN]) as [Id]
    ,[GEOMETRIE] as [Puntgeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[GelijkgrondseKruisingen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[OngelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [OK_OIDN] as [ObjectId]
    ,CONCAT('https://data.vlaanderen.be/id/ongelijkgrondsekruising/', [OK_OIDN]) as [Id]
    ,[GEOMETRIE] as [Puntgeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[OngelijkgrondseKruisingen]
WHERE [GEOMETRIE] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wfs].[Wegsegmenten]', N'V') IS NOT NULL
                    DROP VIEW [wfs].[Wegsegmenten]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wfs].[Wegknopen]', N'V') IS NOT NULL
                    DROP VIEW [wfs].[Wegknopen]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wfs].[GelijkgrondseKruisingen]', N'V') IS NOT NULL
                    DROP VIEW [wfs].[GelijkgrondseKruisingen]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wfs].[OngelijkgrondseKruisingen]', N'V') IS NOT NULL
                    DROP VIEW [wfs].[OngelijkgrondseKruisingen]; ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'wfs')
                    EXEC(N'DROP SCHEMA [wfs]');");
        }
    }
}
