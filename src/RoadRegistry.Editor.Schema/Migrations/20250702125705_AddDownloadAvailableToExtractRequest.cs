using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadAvailableToExtractRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DownloadAvailable",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExtractDownloadTimeoutOccurred",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadAvailable",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");

            migrationBuilder.DropColumn(
                name: "ExtractDownloadTimeoutOccurred",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");
        }
    }
}
