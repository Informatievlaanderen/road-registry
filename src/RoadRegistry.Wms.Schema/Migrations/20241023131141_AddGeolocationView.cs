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
FROM [RoadRegistryWms].[wegsegmentDenorm]");

            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX IX_RoadSegmentGeolocationView_ObjectId ON [geolocation].[RoadSegmentGeolocationView] ([Id])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW geolocation.RoadSegmentGeolocationView;");
        }
    }
}
