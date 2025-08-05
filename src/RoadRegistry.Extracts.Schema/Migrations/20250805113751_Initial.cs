using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryExtracts");

            migrationBuilder.CreateTable(
                name: "ExtractDownloads",
                schema: "RoadRegistryExtracts",
                columns: table => new
                {
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtractRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Contour = table.Column<Geometry>(type: "Geometry", nullable: false),
                    IsInformative = table.Column<bool>(type: "bit", nullable: false),
                    RequestedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DownloadAvailable = table.Column<bool>(type: "bit", nullable: false),
                    ExtractDownloadTimeoutOccurred = table.Column<bool>(type: "bit", nullable: false),
                    DownloadedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ArchiveId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Closed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractDownloads", x => x.DownloadId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ExtractRequests",
                schema: "RoadRegistryExtracts",
                columns: table => new
                {
                    ExtractRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrganizationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CurrentDownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractRequests", x => x.ExtractRequestId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractDownloads_ExtractRequestId",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                column: "ExtractRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractRequests_CurrentDownloadId",
                schema: "RoadRegistryExtracts",
                table: "ExtractRequests",
                column: "CurrentDownloadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractDownloads",
                schema: "RoadRegistryExtracts");

            migrationBuilder.DropTable(
                name: "ExtractRequests",
                schema: "RoadRegistryExtracts");
        }
    }
}
