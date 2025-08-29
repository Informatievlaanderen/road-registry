using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddExtractStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadAvailable",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");

            migrationBuilder.DropColumn(
                name: "ExtractDownloadTimeoutOccurred",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");

            migrationBuilder.AddColumn<int>(
                name: "DownloadStatus",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UploadStatus",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UploadedOn",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadStatus",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");

            migrationBuilder.DropColumn(
                name: "UploadStatus",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");

            migrationBuilder.DropColumn(
                name: "UploadedOn",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");

            migrationBuilder.AddColumn<bool>(
                name: "DownloadAvailable",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExtractDownloadTimeoutOccurred",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
