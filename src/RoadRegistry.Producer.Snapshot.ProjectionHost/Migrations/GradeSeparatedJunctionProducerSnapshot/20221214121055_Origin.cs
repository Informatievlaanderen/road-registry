using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.GradeSeparatedJunctionProducerSnapshot
{
    public partial class Origin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction");

            migrationBuilder.RenameColumn(
                name: "Origin_OrganizationName",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "Origin_Organization");

            migrationBuilder.RenameColumn(
                name: "Origin_BeginTime",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "Origin_Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Origin_Timestamp",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "Origin_BeginTime");

            migrationBuilder.RenameColumn(
                name: "Origin_Organization",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "Origin_OrganizationName");

            migrationBuilder.AddColumn<string>(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
