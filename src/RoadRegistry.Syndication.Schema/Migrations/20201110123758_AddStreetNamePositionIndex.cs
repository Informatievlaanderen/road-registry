using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class AddStreetNamePositionIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StreetName_Position",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                column: "Position");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetName_Position",
                schema: "RoadRegistrySyndication",
                table: "StreetName");
        }
    }
}
