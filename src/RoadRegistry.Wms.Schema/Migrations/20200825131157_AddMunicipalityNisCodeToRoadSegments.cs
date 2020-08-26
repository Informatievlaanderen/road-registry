using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class AddMunicipalityNisCodeToRoadSegments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "linksGemeenteNisCode",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rechtsGemeenteNisCode",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "linksGemeenteNisCode",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropColumn(
                name: "rechtsGemeenteNisCode",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }
    }
}
