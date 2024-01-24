using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    public partial class AddStreetNameIdIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_linkerstraatnaamObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "linkerstraatnaamObjectId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_rechterstraatnaamObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "rechterstraatnaamObjectId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_linkerstraatnaamObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_rechterstraatnaamObjectId",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");
        }
    }
}
