using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class CreatieVersieAsTimestamp : Migration
    {
        // CREATIE/VERSIE become real datetimeoffset columns. The [wms] views are WITH SCHEMABINDING and reference these
        // columns, so they are dropped before the ALTER and recreated afterwards.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[EuropeseWegen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[EuropeseWegen]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[NationaleWegen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[NationaleWegen]; ");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CREATIE",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VERSIE",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CREATIE",
                schema: "road",
                table: "Wegsegmenten",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VERSIE",
                schema: "road",
                table: "Wegsegmenten",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CREATIE",
                schema: "road",
                table: "Wegknopen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VERSIE",
                schema: "road",
                table: "Wegknopen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CREATIE",
                schema: "road",
                table: "GelijkgrondseKruisingen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VERSIE",
                schema: "road",
                table: "GelijkgrondseKruisingen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CREATIE",
                schema: "road",
                table: "OngelijkgrondseKruisingen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VERSIE",
                schema: "road",
                table: "OngelijkgrondseKruisingen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CREATIE",
                schema: "road",
                table: "EuropeseWegen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VERSIE",
                schema: "road",
                table: "EuropeseWegen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CREATIE",
                schema: "road",
                table: "NationaleWegen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VERSIE",
                schema: "road",
                table: "NationaleWegen",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)");

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
CREATE VIEW [wms].[Wegknopen] WITH SCHEMABINDING AS
SELECT
     [WK_OIDN]
    ,[GEOMETRIE]
    ,[TYPE]
    ,[GRENSKNOOP]
    ,[CREATIE]
    ,[VERSIE]
FROM [road].[Wegknopen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[GelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [GK_OIDN]
    ,[GEOMETRIE]
    ,[WS1_OIDN]
    ,[WS2_OIDN]
    ,[CREATIE]
    ,[VERSIE]
FROM [road].[GelijkgrondseKruisingen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[OngelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [OK_OIDN]
    ,[GEOMETRIE]
    ,[BO_WS_OIDN]
    ,[ON_WS_OIDN]
    ,[TYPE]
    ,[CREATIE]
    ,[VERSIE]
FROM [road].[OngelijkgrondseKruisingen]");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[EuropeseWegen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[EuropeseWegen]; ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID (N'[wms].[NationaleWegen]', N'V') IS NOT NULL
                    DROP VIEW [wms].[NationaleWegen]; ");

            migrationBuilder.AlterColumn<string>(
                name: "CREATIE",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "VERSIE",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "CREATIE",
                schema: "road",
                table: "Wegsegmenten",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "VERSIE",
                schema: "road",
                table: "Wegsegmenten",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "CREATIE",
                schema: "road",
                table: "Wegknopen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "VERSIE",
                schema: "road",
                table: "Wegknopen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "CREATIE",
                schema: "road",
                table: "GelijkgrondseKruisingen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "VERSIE",
                schema: "road",
                table: "GelijkgrondseKruisingen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "CREATIE",
                schema: "road",
                table: "OngelijkgrondseKruisingen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "VERSIE",
                schema: "road",
                table: "OngelijkgrondseKruisingen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "CREATIE",
                schema: "road",
                table: "EuropeseWegen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "VERSIE",
                schema: "road",
                table: "EuropeseWegen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "CREATIE",
                schema: "road",
                table: "NationaleWegen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "VERSIE",
                schema: "road",
                table: "NationaleWegen",
                type: "varchar(15)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

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
CREATE VIEW [wms].[Wegknopen] WITH SCHEMABINDING AS
SELECT
     [WK_OIDN]
    ,[GEOMETRIE]
    ,[TYPE]
    ,[GRENSKNOOP]
    ,[CREATIE]
    ,[VERSIE]
FROM [road].[Wegknopen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[GelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [GK_OIDN]
    ,[GEOMETRIE]
    ,[WS1_OIDN]
    ,[WS2_OIDN]
    ,[CREATIE]
    ,[VERSIE]
FROM [road].[GelijkgrondseKruisingen]");

            migrationBuilder.Sql(@"
CREATE VIEW [wms].[OngelijkgrondseKruisingen] WITH SCHEMABINDING AS
SELECT
     [OK_OIDN]
    ,[GEOMETRIE]
    ,[BO_WS_OIDN]
    ,[ON_WS_OIDN]
    ,[TYPE]
    ,[CREATIE]
    ,[VERSIE]
FROM [road].[OngelijkgrondseKruisingen]");

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
