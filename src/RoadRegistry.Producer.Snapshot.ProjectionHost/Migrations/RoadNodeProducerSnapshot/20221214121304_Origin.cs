using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadNodeProducerSnapshot
{
    public partial class Origin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode");

            migrationBuilder.RenameColumn(
                name: "Origin_OrganizationName",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                newName: "Origin_Organization");

            migrationBuilder.RenameColumn(
                name: "Origin_BeginTime",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                newName: "Origin_Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Origin_Timestamp",
                schema: "RoadRegistryRoadNodeProducerSnapshot",
                table: "RoadNode",
                newName: "Origin_BeginTime");

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
        }
    }
}
