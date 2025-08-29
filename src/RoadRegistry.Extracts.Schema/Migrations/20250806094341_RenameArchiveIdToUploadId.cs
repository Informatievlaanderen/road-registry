using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class RenameArchiveIdToUploadId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveId",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");

            migrationBuilder.AddColumn<Guid>(
                name: "UploadId",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadId",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");

            migrationBuilder.AddColumn<string>(
                name: "ArchiveId",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
