using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddZipArchiveWriterVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZipArchiveWriterVersion",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZipArchiveWriterVersion",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload"); 
        }
    }
}
