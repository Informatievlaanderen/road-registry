using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class ModifyRoadNetworkExtractRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DownloadedOn",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DownloadedOn",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload",
                type: "datetimeoffset",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadedOn",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");

            migrationBuilder.DropColumn(
                name: "DownloadedOn",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload");
        }
    }
}
