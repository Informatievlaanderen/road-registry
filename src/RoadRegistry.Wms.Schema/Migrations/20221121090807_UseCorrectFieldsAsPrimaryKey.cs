using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class UseCorrectFieldsAsPrimaryKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NationaleWeg",
                schema: "RoadRegistryWms",
                table: "NationaleWeg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EuropeseWeg",
                schema: "RoadRegistryWms",
                table: "EuropeseWeg");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NationaleWeg",
                schema: "RoadRegistryWms",
                table: "NationaleWeg",
                column: "NW_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EuropeseWeg",
                schema: "RoadRegistryWms",
                table: "EuropeseWeg",
                column: "EU_OIDN")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NationaleWeg",
                schema: "RoadRegistryWms",
                table: "NationaleWeg");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EuropeseWeg",
                schema: "RoadRegistryWms",
                table: "EuropeseWeg");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NationaleWeg",
                schema: "RoadRegistryWms",
                table: "NationaleWeg",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EuropeseWeg",
                schema: "RoadRegistryWms",
                table: "EuropeseWeg",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
