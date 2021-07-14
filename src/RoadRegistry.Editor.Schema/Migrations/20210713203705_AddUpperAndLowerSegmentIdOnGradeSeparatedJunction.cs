using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddUpperAndLowerSegmentIdOnGradeSeparatedJunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GradeSeparatedJunction",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction");

            migrationBuilder.AddColumn<int>(
                name: "LowerRoadSegmentId",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpperRoadSegmentId",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GradeSeparatedJunction",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction",
                column: "Id")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GradeSeparatedJunction",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction");

            migrationBuilder.DropColumn(
                name: "LowerRoadSegmentId",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction");

            migrationBuilder.DropColumn(
                name: "UpperRoadSegmentId",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GradeSeparatedJunction",
                schema: "RoadRegistryEditor",
                table: "GradeSeparatedJunction",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
