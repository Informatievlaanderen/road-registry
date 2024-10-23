using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddGeolocationView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "geolocation");
            migrationBuilder.Sql("ALTER AUTHORIZATION ON SCHEMA::geolocation TO wms");
            migrationBuilder.Sql(@"
CREATE VIEW geolocation.RoadSegmentOsloGeolocationView WITH SCHEMABINDING AS
SELECT
      CASE
            WHEN [geometrie2D].STGeometryType() = 'LINESTRING' THEN 'MULTILINESTRING (' + [geometrie2D].STAsText() + ')'
            ELSE [geometrie2D].STAsText()
      END AS [GEOMETRYASWKT]
      ,[linksGemeenteNisCode] as [LEFTSIDEMUNICIPALITYNISCODE]
      ,[linksStraatnaam] as [LEFTSIDESTREETNAME]
      ,[linksStraatnaamID] as [LEFTSIDESTREETNAMEID]
      ,[morfologie] as [MORPHOLOGYID]
      ,[rechtsGemeenteNisCode] as [RIGHTSIDEMUNICIPALITYNISCODE]
      ,[rechtsStraatnaam] as [RIGHTSIDESTREETNAME]
      ,[rechtsStraatnaamID] as [RIGHTSIDESTREETNAMEID]
      ,[wegsegmentID] as Id
FROM [RoadRegistryWms].[wegsegmentDenorm]");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_RoadSegmentGeolocationView_ObjectId ON [geolocation].[RoadSegmentOsloGeolocationView] (Id)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW geolocation.RoadSegmentOsloGeolocationView;");
        }
    }
}
