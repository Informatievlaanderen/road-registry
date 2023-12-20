using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Product.Schema.Migrations
{
    public partial class AddOrganizationDbaseSchemaVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DbaseSchemaVersion",
                schema: "RoadRegistryProduct",
                table: "Organization",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "V1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DbaseSchemaVersion",
                schema: "RoadRegistryProduct",
                table: "Organization");
        }
    }
}
