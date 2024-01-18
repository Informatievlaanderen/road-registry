using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    public partial class RemoveStreetNameCachePosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreetNameCachePosition",
                schema: "RoadRegistryWfs",
                table: "Wegsegment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StreetNameCachePosition",
                schema: "RoadRegistryWfs",
                table: "Wegsegment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
