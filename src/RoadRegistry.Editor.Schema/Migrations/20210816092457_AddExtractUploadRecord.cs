using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddExtractUploadRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtractUpload",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    UploadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalRequestId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArchiveId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceivedOn = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedOn = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractUpload", x => x.UploadId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractUpload_ChangeRequestId",
                schema: "RoadRegistryEditor",
                table: "ExtractUpload",
                column: "ChangeRequestId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractUpload",
                schema: "RoadRegistryEditor");
        }
    }
}
