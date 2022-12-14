using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentSurfaceProducerSnapshot
{
    public partial class Origin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema",
                table: "RoadSegmentSurface");

            migrationBuilder.RenameColumn(
                name: "Origin_OrganizationName",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema",
                table: "RoadSegmentSurface",
                newName: "Origin_Organization");

            migrationBuilder.RenameColumn(
                name: "Origin_BeginTime",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema",
                table: "RoadSegmentSurface",
                newName: "Origin_Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Origin_Timestamp",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema",
                table: "RoadSegmentSurface",
                newName: "Origin_BeginTime");

            migrationBuilder.RenameColumn(
                name: "Origin_Organization",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema",
                table: "RoadSegmentSurface",
                newName: "Origin_OrganizationName");

            migrationBuilder.AddColumn<string>(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema",
                table: "RoadSegmentSurface",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
