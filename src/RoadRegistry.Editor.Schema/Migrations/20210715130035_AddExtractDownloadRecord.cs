using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddExtractDownloadRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtractDownload",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalRequestId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchiveId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedOn = table.Column<long>(type: "bigint", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false),
                    AvailableOn = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractDownload", x => x.DownloadId)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractDownload",
                schema: "RoadRegistryEditor");
        }
    }
}
