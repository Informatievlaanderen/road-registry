using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class AddRoadSegmentIsRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "verwijderd",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_verwijderd",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "verwijderd")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_verwijderd",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropColumn(
                name: "verwijderd",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }
    }
}
