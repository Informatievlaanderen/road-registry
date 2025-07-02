using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveIdToExtractRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchiveId",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveId",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");
        }
    }
}
