using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddMunicipalityGeometry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MunicipalityGeometry",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    NisCode = table.Column<string>(nullable: false),
                    Geometry = table.Column<Geometry>(type: "Geometry", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityGeometry", x => x.NisCode)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalityGeometry",
                schema: "RoadRegistryEditor");
        }
    }
}
