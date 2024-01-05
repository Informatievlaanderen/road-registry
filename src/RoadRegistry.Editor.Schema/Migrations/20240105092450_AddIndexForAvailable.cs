using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddIndexForAvailable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ExtractDownload_Available",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload",
                column: "Available");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExtractDownload_Available",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload");
        }
    }
}
