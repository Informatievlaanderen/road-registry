using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class RemoveUnusedRoadNodeAndRoadSegmentBoundingBoxTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadNodeBoundingBox"
            );

            migrationBuilder.DropTable(
                name: "RoadSegmentBoundingBox"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadNodeBoundingBox",
                columns: table => new
                {
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentBoundingBox",
                columns: table => new
                {
                    MaximumM = table.Column<double>(type: "float", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    MinimumM = table.Column<double>(type: "float", nullable: false),
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}
