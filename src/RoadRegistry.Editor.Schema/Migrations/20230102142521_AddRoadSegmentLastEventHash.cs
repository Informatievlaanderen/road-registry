using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddRoadSegmentLastEventHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastEventHash",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "When",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkChange",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadNodeBoundingBox");

            migrationBuilder.DropTable(
                name: "RoadSegmentBoundingBox");

            migrationBuilder.DropColumn(
                name: "LastEventHash",
                schema: "RoadRegistryEditor",
                table: "RoadSegment");

            migrationBuilder.AlterColumn<string>(
                name: "When",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkChange",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
