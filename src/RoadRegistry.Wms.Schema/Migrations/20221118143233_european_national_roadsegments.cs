using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class european_national_roadsegments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EuropeseWeg",
                schema: "RoadRegistryWms",
                columns: table => new
                {
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    BEGINORG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BEGINTIJD = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EU_OIDN = table.Column<int>(type: "int", nullable: false),
                    EUNUMMER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LBLBGNORG = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeseWeg", x => x.WS_OIDN)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "NationaleWeg",
                schema: "RoadRegistryWms",
                columns: table => new
                {
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    BEGINORG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BEGINTIJD = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IDENT2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LBLBGNORG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NW_OIDN = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationaleWeg", x => x.WS_OIDN)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EuropeseWeg",
                schema: "RoadRegistryWms");

            migrationBuilder.DropTable(
                name: "NationaleWeg",
                schema: "RoadRegistryWms");
        }
    }
}
