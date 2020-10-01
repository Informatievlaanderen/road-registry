using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class AddIndexToStreetNamePersistentLocalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StreetName_PersistentLocalId",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                column: "PersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetName_PersistentLocalId",
                schema: "RoadRegistrySyndication",
                table: "StreetName");
        }
    }
}
