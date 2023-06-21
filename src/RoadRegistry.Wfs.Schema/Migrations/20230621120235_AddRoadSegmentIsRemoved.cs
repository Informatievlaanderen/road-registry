using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    public partial class AddRoadSegmentIsRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "verwijderd",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Wegsegment_verwijderd",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                column: "verwijderd")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wegsegment_verwijderd",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");

            migrationBuilder.DropColumn(
                name: "verwijderd",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");
        }
    }
}
