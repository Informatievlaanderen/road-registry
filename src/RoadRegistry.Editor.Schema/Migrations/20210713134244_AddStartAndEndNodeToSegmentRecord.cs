using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddStartAndEndNodeToSegmentRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndNodeId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartNodeId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndNodeId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "StartNodeId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment");
        }
    }
}
