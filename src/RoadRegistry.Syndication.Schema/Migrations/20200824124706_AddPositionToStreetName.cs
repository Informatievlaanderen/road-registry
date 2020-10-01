using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class AddPositionToStreetName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Position",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                schema: "RoadRegistrySyndication",
                table: "StreetName");
        }
    }
}
