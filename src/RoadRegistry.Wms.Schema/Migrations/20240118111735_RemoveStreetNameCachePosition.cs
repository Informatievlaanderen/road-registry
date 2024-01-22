using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class RemoveStreetNameCachePosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropColumn(
                name: "straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "straatnaamCachePositie")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
