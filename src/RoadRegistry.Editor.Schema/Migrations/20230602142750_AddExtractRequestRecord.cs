using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddExtractRequestRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInformative",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExtractRequest",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contour = table.Column<Geometry>(type: "Geometry", nullable: false),
                    ExternalRequestId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsInformative = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractRequest", x => x.DownloadId)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractRequest",
                schema: "RoadRegistryEditor");

            migrationBuilder.DropColumn(
                name: "IsInformative",
                schema: "RoadRegistryEditor",
                table: "ExtractDownload");
        }
    }
}
