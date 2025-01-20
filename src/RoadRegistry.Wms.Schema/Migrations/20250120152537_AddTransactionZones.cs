using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bijwerkingszones",
                schema: "RoadRegistryWmsMeta",
                columns: table => new
                {
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Omschrijving = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contour = table.Column<Geometry>(type: "Geometry", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bijwerkingszones", x => x.DownloadId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "OverlappendeBijwerkingszones",
                schema: "RoadRegistryWmsMeta",
                columns: table => new
                {
                    DownloadId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadId2 = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Contour = table.Column<Geometry>(type: "Geometry", nullable: false),
                    Omschrijving1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Omschrijving2 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverlappendeBijwerkingszones", x => new { x.DownloadId1, x.DownloadId2 })
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OverlappendeBijwerkingszones_DownloadId1",
                schema: "RoadRegistryWmsMeta",
                table: "OverlappendeBijwerkingszones",
                column: "DownloadId1");

            migrationBuilder.CreateIndex(
                name: "IX_OverlappendeBijwerkingszones_DownloadId2",
                schema: "RoadRegistryWmsMeta",
                table: "OverlappendeBijwerkingszones",
                column: "DownloadId2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bijwerkingszones",
                schema: "RoadRegistryWmsMeta");

            migrationBuilder.DropTable(
                name: "OverlappendeBijwerkingszones",
                schema: "RoadRegistryWmsMeta");
        }
    }
}
