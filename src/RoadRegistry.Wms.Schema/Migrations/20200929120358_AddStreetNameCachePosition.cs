using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class AddStreetNameCachePosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }
    }
}
