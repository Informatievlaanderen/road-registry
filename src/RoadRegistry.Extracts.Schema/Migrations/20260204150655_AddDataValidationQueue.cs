using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddDataValidationQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtractUploads",
                schema: "RoadRegistryExtracts",
                columns: table => new
                {
                    UploadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractUploads", x => x.UploadId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.Sql(@"
INSERT INTO [RoadRegistryExtracts].[ExtractUploads] (UploadId, DownloadId, UploadedOn, Status, TicketId)
SELECT UploadId, DownloadId, UploadedOn, UploadStatus, COALESCE(TicketId, CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier))
FROM [RoadRegistryExtracts].[ExtractDownloads]
WHERE UploadId IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "UploadStatus",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");
            migrationBuilder.DropColumn(
                name: "UploadedOn",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads");
            migrationBuilder.RenameColumn(
                name: "UploadId",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                newName: "LatestUploadId");
            migrationBuilder.RenameColumn(
                name: "DownloadStatus",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                newName: "Status");

            migrationBuilder.CreateTable(
                name: "DataValidationQueue",
                schema: "RoadRegistryExtracts",
                columns: table => new
                {
                    UploadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SqsRequestJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataValidationId = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    Completed = table.Column<bool>(type: "bit", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataValidationQueue", x => x.UploadId)
                        .Annotation("SqlServer:Clustered", true);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataValidationQueue",
                schema: "RoadRegistryExtracts");

            migrationBuilder.DropTable(
                name: "ExtractUploads",
                schema: "RoadRegistryExtracts");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                newName: "DownloadStatus");

            migrationBuilder.RenameColumn(
                name: "LatestUploadId",
                schema: "RoadRegistryExtracts",
                table: "ExtractDownloads",
                newName: "UploadId");

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
    }
}
