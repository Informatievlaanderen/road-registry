using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddExtractRequestOverlap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtractRequestOverlap",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contour = table.Column<Geometry>(type: "Geometry", nullable: false),
                    DownloadId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadId2 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description2 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractRequestOverlap", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractRequestOverlap",
                schema: "RoadRegistryEditor");
        }
    }
}
