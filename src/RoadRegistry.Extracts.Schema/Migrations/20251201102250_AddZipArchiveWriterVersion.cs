using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddZipArchiveWriterVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZipArchiveWriterVersion",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "V2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZipArchiveWriterVersion",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");
        }
    }
}
