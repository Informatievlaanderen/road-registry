using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class MakeIdOfNodeAndSegmentClustered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadSegment",
                schema: "RoadRegistryEditor",
                table: "RoadSegment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadNode",
                schema: "RoadRegistryEditor",
                table: "RoadNode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoadSegment",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "Id")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoadNode",
                schema: "RoadRegistryEditor",
                table: "RoadNode",
                column: "Id")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadSegment",
                schema: "RoadRegistryEditor",
                table: "RoadSegment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadNode",
                schema: "RoadRegistryEditor",
                table: "RoadNode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoadSegment",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoadNode",
                schema: "RoadRegistryEditor",
                table: "RoadNode",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
