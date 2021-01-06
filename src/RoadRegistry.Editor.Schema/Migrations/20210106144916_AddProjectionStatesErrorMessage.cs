using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddProjectionStatesErrorMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "RoadRegistryEditorMeta",
                table: "ProjectionStates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoadNodeBoundingBox",
                columns: table => new
                {
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentBoundingBox",
                columns: table => new
                {
                    MinimumX = table.Column<double>(type: "float", nullable: false),
                    MaximumX = table.Column<double>(type: "float", nullable: false),
                    MinimumY = table.Column<double>(type: "float", nullable: false),
                    MaximumY = table.Column<double>(type: "float", nullable: false),
                    MinimumM = table.Column<double>(type: "float", nullable: false),
                    MaximumM = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadNodeBoundingBox");

            migrationBuilder.DropTable(
                name: "RoadSegmentBoundingBox");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "RoadRegistryEditorMeta",
                table: "ProjectionStates");
        }
    }
}
