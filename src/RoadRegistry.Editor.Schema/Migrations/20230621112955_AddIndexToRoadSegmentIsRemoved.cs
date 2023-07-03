using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddIndexToRoadSegmentIsRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_IsRemoved",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "IsRemoved")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoadSegment_IsRemoved",
                schema: "RoadRegistryEditor",
                table: "RoadSegment");
        }
    }
}
