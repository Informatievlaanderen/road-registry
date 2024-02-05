using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddMaintainerIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaintainerId",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentV2",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentV2_MaintainerId",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentV2",
                column: "MaintainerId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoadSegmentV2_MaintainerId",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentV2");

            migrationBuilder.AlterColumn<string>(
                name: "MaintainerId",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentV2",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
