using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class WfsViewsIdsStatusMethode : Migration
    {
        // The [wfs] views name their identifier after the entity (WegsegmentId, WegknoopId, ...) instead of a generic
        // ObjectId, and [wfs].[Wegsegmenten] also exposes the segment status and geometry method, by their label.
        //
        // A view cannot have a column renamed, so each view is dropped and created again. [wfs].[Wegsegmenten] and
        // [wfs].[Wegknopen] are indexed views: dropping the view drops its unique clustered index with it, so that index
        // is created again on the renamed column.

        private static readonly (string View, string Column)[] IndexedViews =
        {
            ("Wegsegmenten", "WegsegmentId"),
            ("Wegknopen", "WegknoopId")
        };

        private static readonly (string View, string Column)[] PreviousIndexedViews =
        {
            ("Wegsegmenten", "ObjectId"),
            ("Wegknopen", "ObjectId")
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropViews(migrationBuilder);

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[Wegsegmenten] WITH SCHEMABINDING AS
SELECT
     [WS_OIDN] as [WegsegmentId]
    ,CONCAT('https://data.vlaanderen.be/id/wegsegment/', [WS_OIDN]) as [Id]
    ,[LBLSTATUS] as [Wegsegmentstatus]
    ,[LBLMETHODE] as [Geometriemethode]
    ,[GEOMETRIE] as [Lijngeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[Wegsegmenten]");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[Wegknopen] WITH SCHEMABINDING AS
SELECT
     [WK_OIDN] as [WegknoopId]
    ,CONCAT('https://data.vlaanderen.be/id/wegknoop/', [WK_OIDN]) as [Id]
    ,[GEOMETRIE] as [Puntgeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[Wegknopen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[GelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [GK_OIDN] as [GelijkgrondsekruisingId]
    ,CONCAT('https://data.vlaanderen.be/id/gelijkgrondsekruising/', [GK_OIDN]) as [Id]
    ,[GEOMETRIE] as [Puntgeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[GelijkgrondseKruisingen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wfs].[OngelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [OK_OIDN] as [OngelijkgrondsekruisingId]
    ,CONCAT('https://data.vlaanderen.be/id/ongelijkgrondsekruising/', [OK_OIDN]) as [Id]
    ,[GEOMETRIE] as [Puntgeometrie]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[OngelijkgrondseKruisingen]
WHERE [GEOMETRIE] IS NOT NULL");

            CreateIndexes(migrationBuilder, IndexedViews);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropViews(migrationBuilder);

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

            CreateIndexes(migrationBuilder, PreviousIndexedViews);
        }

        private static void DropViews(MigrationBuilder migrationBuilder)
        {
            foreach (var view in new[] { "Wegsegmenten", "Wegknopen", "GelijkgrondseKruisingen", "OngelijkgrondseKruisingen" })
            {
                migrationBuilder.Sql($@"
                IF OBJECT_ID (N'[wfs].[{view}]', N'V') IS NOT NULL
                    DROP VIEW [wfs].[{view}]; ");
            }
        }

        private static void CreateIndexes(MigrationBuilder migrationBuilder, (string View, string Column)[] indexedViews)
        {
            foreach (var (view, column) in indexedViews)
            {
                migrationBuilder.Sql($@"
CREATE UNIQUE CLUSTERED INDEX [IX_{view}_{column}] ON [wfs].[{view}]
(
    [{column}]
);");
            }
        }
    }
}
