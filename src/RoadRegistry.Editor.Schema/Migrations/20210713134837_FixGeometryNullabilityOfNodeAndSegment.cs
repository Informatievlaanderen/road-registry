using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class FixGeometryNullabilityOfNodeAndSegment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Geometry>(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                type: "Geometry",
                nullable: false,
                oldClrType: typeof(Geometry),
                oldType: "Geometry",
                oldNullable: true);

            migrationBuilder.AlterColumn<Geometry>(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadNode",
                type: "Geometry",
                nullable: false,
                oldClrType: typeof(Geometry),
                oldType: "Geometry",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Geometry>(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                type: "Geometry",
                nullable: true,
                oldClrType: typeof(Geometry),
                oldType: "Geometry");

            migrationBuilder.AlterColumn<Geometry>(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadNode",
                type: "Geometry",
                nullable: true,
                oldClrType: typeof(Geometry),
                oldType: "Geometry");
        }
    }
}
