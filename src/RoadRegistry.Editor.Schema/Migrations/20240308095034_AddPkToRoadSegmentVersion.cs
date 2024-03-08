using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddPkToRoadSegmentVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StreamId",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoadSegmentVersion",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion",
                columns: new[] { "StreamId", "Id", "Method" })
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadSegmentVersion",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion");

            migrationBuilder.AlterColumn<string>(
                name: "StreamId",
                schema: "RoadRegistryEditor",
                table: "RoadSegmentVersion",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
