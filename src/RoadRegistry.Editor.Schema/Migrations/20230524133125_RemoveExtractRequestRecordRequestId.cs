using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class RemoveExtractRequestRecordRequestId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestId",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
