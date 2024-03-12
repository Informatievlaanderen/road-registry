using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Jobs.Migrations
{
    public partial class AddUploadTypeToJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DownloadId",
                schema: "RoadRegistryJobs",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UploadType",
                schema: "RoadRegistryJobs",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadId",
                schema: "RoadRegistryJobs",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UploadType",
                schema: "RoadRegistryJobs",
                table: "Jobs");
        }
    }
}
