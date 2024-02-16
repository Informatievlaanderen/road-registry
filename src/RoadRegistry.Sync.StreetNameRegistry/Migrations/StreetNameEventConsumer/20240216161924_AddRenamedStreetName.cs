using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Sync.StreetNameRegistry.Migrations.StreetNameEventConsumer
{
    public partial class AddRenamedStreetName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryStreetName");

            migrationBuilder.CreateTable(
                name: "RenamedStreetName",
                schema: "RoadRegistryStreetName",
                columns: table => new
                {
                    StreetNameLocalId = table.Column<int>(type: "int", nullable: false),
                    DestinationStreetNameLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenamedStreetName", x => x.StreetNameLocalId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenamedStreetName_StreetNameLocalId",
                schema: "RoadRegistryStreetName",
                table: "RenamedStreetName",
                column: "StreetNameLocalId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RenamedStreetName",
                schema: "RoadRegistryStreetName");
        }
    }
}
