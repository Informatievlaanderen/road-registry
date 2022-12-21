using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.GradeSeparatedJunctionProducerSnapshot
{
    public partial class TypeIdDutchName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "Origin_Organization",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "TypeDutchName");

            migrationBuilder.AddColumn<string>(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Origin_OrganizationName",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin_OrganizationId",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction");

            migrationBuilder.DropColumn(
                name: "Origin_OrganizationName",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "TypeDutchName",
                schema: "RoadRegistryGradeSeparatedJunctionProducerSnapshotSchema",
                table: "GradeSeparatedJunction",
                newName: "Origin_Organization");
        }
    }
}
