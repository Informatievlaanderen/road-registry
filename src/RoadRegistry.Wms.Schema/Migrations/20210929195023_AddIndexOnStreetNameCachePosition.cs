using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class AddIndexOnStreetNameCachePosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "straatnaamCachePositie")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }
    }
}
