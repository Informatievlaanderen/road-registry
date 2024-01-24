using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class AddStreetNameIdIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_linksStraatnaamID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "linksStraatnaamID")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_rechtsStraatnaamID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "rechtsStraatnaamID")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_linksStraatnaamID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "IX_wegsegmentDenorm_rechtsStraatnaamID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }
    }
}
