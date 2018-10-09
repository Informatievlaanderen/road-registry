using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Projections.Migrations
{
    public partial class AddShapeEnvelopeToShapeProjections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Envelope_MaximumX",
                schema: "RoadRegistryShape",
                table: "RoadSegment",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MaximumY",
                schema: "RoadRegistryShape",
                table: "RoadSegment",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MinimumX",
                schema: "RoadRegistryShape",
                table: "RoadSegment",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MinimumY",
                schema: "RoadRegistryShape",
                table: "RoadSegment",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MaximumX",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MaximumY",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MinimumX",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MinimumY",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MaximumX",
                schema: "RoadRegistryShape",
                table: "RoadNode",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MaximumY",
                schema: "RoadRegistryShape",
                table: "RoadNode",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MinimumX",
                schema: "RoadRegistryShape",
                table: "RoadNode",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Envelope_MinimumY",
                schema: "RoadRegistryShape",
                table: "RoadNode",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Envelope_MaximumX",
                schema: "RoadRegistryShape",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "Envelope_MaximumY",
                schema: "RoadRegistryShape",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "Envelope_MinimumX",
                schema: "RoadRegistryShape",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "Envelope_MinimumY",
                schema: "RoadRegistryShape",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "Envelope_MaximumX",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint");

            migrationBuilder.DropColumn(
                name: "Envelope_MaximumY",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint");

            migrationBuilder.DropColumn(
                name: "Envelope_MinimumX",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint");

            migrationBuilder.DropColumn(
                name: "Envelope_MinimumY",
                schema: "RoadRegistryShape",
                table: "RoadReferencePoint");

            migrationBuilder.DropColumn(
                name: "Envelope_MaximumX",
                schema: "RoadRegistryShape",
                table: "RoadNode");

            migrationBuilder.DropColumn(
                name: "Envelope_MaximumY",
                schema: "RoadRegistryShape",
                table: "RoadNode");

            migrationBuilder.DropColumn(
                name: "Envelope_MinimumX",
                schema: "RoadRegistryShape",
                table: "RoadNode");

            migrationBuilder.DropColumn(
                name: "Envelope_MinimumY",
                schema: "RoadRegistryShape",
                table: "RoadNode");
        }
    }
}
