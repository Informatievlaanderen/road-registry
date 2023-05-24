using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class RemoveExtractRequestRecordAvailableOn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Available",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");

            migrationBuilder.DropColumn(
                name: "AvailableOn",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Available",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "AvailableOn",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
