using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class OngelijkgrondseKruisingenViewOnlyWithGeometry : Migration
    {
        // A grade separated junction is only known once both linked road segments are present, so its geometry (the
        // intersection point of those segments) can be null. Such rows cannot be rendered, so the WMS view exposes only
        // the junctions that actually have a geometry. The column itself stays nullable in the underlying table.
        private const string ViewColumns = @"
SELECT
     [OK_OIDN] as [OngelijkgrondseKruisingId]
    ,[GEOMETRIE] as [Geometrie]
    ,[BO_WS_OIDN] as [BovenliggendWegsegment]
    ,[ON_WS_OIDN] as [OnderliggendWegsegment]
    ,[TYPE] as [OngelijkgrondseKruisingTypeId]
    ,[LBLTYPE] as [OngelijkgrondseKruisingType]
    ,[CREATIE] as [Creatie]
    ,[VERSIE] as [Versie]
FROM [road].[OngelijkgrondseKruisingen]";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                    DROP VIEW [wms].[OngelijkgrondseKruisingen]; ");

            migrationBuilder.Sql($@"
CREATE VIEW [wms].[OngelijkgrondseKruisingen] WITH SCHEMABINDING AS{ViewColumns}
WHERE [GEOMETRIE] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                    DROP VIEW [wms].[OngelijkgrondseKruisingen]; ");

            migrationBuilder.Sql($@"
CREATE VIEW [wms].[OngelijkgrondseKruisingen] WITH SCHEMABINDING AS{ViewColumns}");
        }
    }
}
