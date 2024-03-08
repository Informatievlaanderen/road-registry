using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddRoadSegmentVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadSegmentVersion",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    StreamId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    GeometryVersion = table.Column<int>(type: "int", nullable: false),
                    RecordingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentVersion_Id",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentVersion_IsRemoved",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion",
                column: "IsRemoved")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentVersion_Method",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion",
                column: "Method")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentVersion_StreamId",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion",
                column: "StreamId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadSegmentVersion",
                schema: "RoadRegistryEditor");
        }
    }
}
