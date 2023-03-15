using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadNodeProducerSnapshot
{
    public partial class Version : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode");
        }
    }
}
