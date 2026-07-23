using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class WegsegmentenDenormalizeLabels : Migration
    {
        // Denormalizes the street-name (LSTRNM/RSTRNM/STRNM) and maintainer (LBLLBEHEER/LBLRBEHEER/LBLBEHEER) labels:
        // they move from CASE/JOIN expressions in [wms].[Wegsegmenten] into stored, indexed columns on
        // [road].[AfgeleideWegsegmenten], maintained by the projections, and are backfilled from the current cache data.
        // The view then reads everything straight from the table (no joins). [wms].[EuropeseWegen] and
        // [wms].[NationaleWegen] are dropped.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The [wms] views are WITH SCHEMABINDING; drop the ones bound to AfgeleideWegsegmenten before altering it.
            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[Wegsegmenten]', N'V') IS NOT NULL
                    DROP VIEW [wms].[Wegsegmenten]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[EuropeseWegen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[EuropeseWegen]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[NationaleWegen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[NationaleWegen]; ");

            migrationBuilder.AddColumn<string>(
                name: "LSTRNM",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RSTRNM",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "STRNM",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(512)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LBLLBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LBLRBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LBLBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(32)",
                nullable: true);

            // Backfill the new columns from the current cache data (mirrors the CASE/JOIN logic the view used to run).
            migrationBuilder.Sql(@"
UPDATE ws SET
     ws.[LSTRNM] = lsn.[Naam]
    ,ws.[RSTRNM] = rsn.[Naam]
    ,ws.[STRNM] = CASE
        WHEN ws.[LSTRNMID] = ws.[RSTRNMID] THEN lsn.[Naam]
        WHEN lsn.[Naam] IS NULL AND rsn.[Naam] IS NULL THEN NULL
        WHEN lsn.[Naam] IS NOT NULL AND rsn.[Naam] IS NULL THEN CONCAT('L: ', lsn.[Naam])
        WHEN rsn.[Naam] IS NOT NULL AND lsn.[Naam] IS NULL THEN CONCAT('R: ', rsn.[Naam])
        ELSE CONCAT('L: ', lsn.[Naam], ' / R: ', rsn.[Naam])
     END
    ,ws.[LBLLBEHEER] = lorg.[Naam]
    ,ws.[LBLRBEHEER] = rorg.[Naam]
    ,ws.[LBLBEHEER] = CASE
        WHEN ws.[STATUS] = 11 AND ws.[LBEHEER] LIKE 'AWV%' AND ws.[RBEHEER] LIKE 'AWV%'
            THEN 'AWV'
        WHEN ws.[STATUS] = 11
             AND (lorg.[Naam] LIKE 'Gemeente%' OR lorg.[Naam] LIKE 'Stad%')
             AND (rorg.[Naam] LIKE 'Gemeente%' OR rorg.[Naam] LIKE 'Stad%')
            THEN 'Steden en gemeenten'
        WHEN ws.[STATUS] = 11 AND ws.[LBEHEER] = 'PARTIC' AND ws.[RBEHEER] = 'PARTIC'
            THEN 'Particulieren'
        WHEN ws.[STATUS] = 11
            THEN 'Andere'
     END
FROM [road].[AfgeleideWegsegmenten] ws
LEFT JOIN [road].[StraatnaamCache] lsn ON lsn.[StraatnaamId] = ws.[LSTRNMID]
LEFT JOIN [road].[StraatnaamCache] rsn ON rsn.[StraatnaamId] = ws.[RSTRNMID]
LEFT JOIN [road].[OrganisatieCache] lorg ON lorg.[OrganisatieId] = ws.[LBEHEER]
LEFT JOIN [road].[OrganisatieCache] rorg ON rorg.[OrganisatieId] = ws.[RBEHEER]");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_STRNM",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "STRNM");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBLBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBLBEHEER");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LSTRNMID",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LSTRNMID");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_RSTRNMID",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "RSTRNMID");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_LBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "LBEHEER");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_RBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "RBEHEER");

            // All labels are stored on the table now, so the view is a plain projection with no cache joins.
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[Wegsegmenten]', N'V') IS NOT NULL
                    DROP VIEW [wms].[Wegsegmenten]; ");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_STRNM",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBLBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LSTRNMID",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_RSTRNMID",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_LBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropIndex(
                name: "IX_AfgeleideWegsegmenten_RBEHEER",
                schema: "road",
                table: "AfgeleideWegsegmenten");

            migrationBuilder.DropColumn(name: "LSTRNM", schema: "road", table: "AfgeleideWegsegmenten");
            migrationBuilder.DropColumn(name: "RSTRNM", schema: "road", table: "AfgeleideWegsegmenten");
            migrationBuilder.DropColumn(name: "STRNM", schema: "road", table: "AfgeleideWegsegmenten");
            migrationBuilder.DropColumn(name: "LBLLBEHEER", schema: "road", table: "AfgeleideWegsegmenten");
            migrationBuilder.DropColumn(name: "LBLRBEHEER", schema: "road", table: "AfgeleideWegsegmenten");
            migrationBuilder.DropColumn(name: "LBLBEHEER", schema: "road", table: "AfgeleideWegsegmenten");

            // Restore the view with the STRNM/LBLBEHEER CASE expressions and the cache joins.
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
    ,CASE
        WHEN ws.[LSTRNMID] = ws.[RSTRNMID] THEN lsn.[Naam]
        WHEN lsn.[Naam] IS NULL AND rsn.[Naam] IS NULL THEN NULL
        WHEN lsn.[Naam] IS NOT NULL AND rsn.[Naam] IS NULL THEN CONCAT('L: ', lsn.[Naam])
        WHEN rsn.[Naam] IS NOT NULL AND lsn.[Naam] IS NULL THEN CONCAT('R: ', rsn.[Naam])
        ELSE CONCAT('L: ', lsn.[Naam], ' / R: ', rsn.[Naam])
     END AS [STRNM]
    ,ws.[LBEHEER]
    ,ws.[RBEHEER]
    ,lorg.[Naam] AS [LBLLBEHEER]
    ,rorg.[Naam] AS [LBLRBEHEER]
    ,CASE
        WHEN ws.[STATUS] = 11 AND ws.[LBEHEER] LIKE 'AWV%' AND ws.[RBEHEER] LIKE 'AWV%'
            THEN 'AWV'
        WHEN ws.[STATUS] = 11
             AND (lorg.[Naam] LIKE 'Gemeente%' OR lorg.[Naam] LIKE 'Stad%')
             AND (rorg.[Naam] LIKE 'Gemeente%' OR rorg.[Naam] LIKE 'Stad%')
            THEN 'Steden en gemeenten'
        WHEN ws.[STATUS] = 11 AND ws.[LBEHEER] = 'PARTIC' AND ws.[RBEHEER] = 'PARTIC'
            THEN 'Particulieren'
        WHEN ws.[STATUS] = 11
            THEN 'Andere'
     END AS [LBLBEHEER]
    ,ws.[AUTOHEEN]
    ,ws.[AUTOTERUG]
    ,ws.[FIETSHEEN]
    ,ws.[FIETSTERUG]
    ,ws.[VOETGANGER]
    ,ws.[EUNUMMERS]
    ,ws.[NWNUMMERS]
    ,ws.[CREATIE]
    ,ws.[VERSIE]
FROM [road].[AfgeleideWegsegmenten] ws
LEFT JOIN [road].[StraatnaamCache] lsn ON lsn.[StraatnaamId] = ws.[LSTRNMID]
LEFT JOIN [road].[StraatnaamCache] rsn ON rsn.[StraatnaamId] = ws.[RSTRNMID]
LEFT JOIN [road].[OrganisatieCache] lorg ON lorg.[OrganisatieId] = ws.[LBEHEER]
LEFT JOIN [road].[OrganisatieCache] rorg ON rorg.[OrganisatieId] = ws.[RBEHEER]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[EuropeseWegen] WITH SCHEMABINDING AS
SELECT
     eu.[EU_OIDN]
    ,ws.[WS_TEMPID]
    ,eu.[EUNUMMER]
    ,eu.[CREATIE]
    ,eu.[VERSIE]
FROM [road].[EuropeseWegen] eu
JOIN [road].[AfgeleideWegsegmenten] ws ON eu.[WS_OIDN] = ws.[WS_OIDN]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[NationaleWegen] WITH SCHEMABINDING AS
SELECT
     nw.[NW_OIDN]
    ,ws.[WS_TEMPID]
    ,nw.[NWNUMMER]
    ,nw.[CREATIE]
    ,nw.[VERSIE]
FROM [road].[NationaleWegen] nw
JOIN [road].[AfgeleideWegsegmenten] ws ON nw.[WS_OIDN] = ws.[WS_OIDN]");
        }
    }
}
