using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class RemoveColumnOperator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "beginoperator",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "beginoperator",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
