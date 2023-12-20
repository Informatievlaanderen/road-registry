using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.StreetNameConsumer.Schema.Migrations
{
    public partial class AddOffset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Offset",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Offset",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");
        }
    }
}
