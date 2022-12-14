using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadNodeProducerSnapshot
{
    public partial class TypeIdDutchName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                newName: "TypeDutchName");

            migrationBuilder.RenameColumn(
                name: "Origin_Organization",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                newName: "Origin_OrganizationName");

            migrationBuilder.AddColumn<string>(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode");

            migrationBuilder.DropColumn(
                name: "TypeId",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode");

            migrationBuilder.RenameColumn(
                name: "TypeDutchName",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Origin_OrganizationName",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                newName: "Origin_Organization");
        }
    }
}
