using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class MakeIdOfOrganisationClustered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organization_Id",
                schema: "RoadRegistryEditor",
                table: "Organization");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Id",
                schema: "RoadRegistryEditor",
                table: "Organization",
                column: "Id")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organization_Id",
                schema: "RoadRegistryEditor",
                table: "Organization");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Id",
                schema: "RoadRegistryEditor",
                table: "Organization",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
