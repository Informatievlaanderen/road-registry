using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddPartOfXRoadsLengthsToRoadNetworkInfoSegmentCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartOfEuropeanRoadsLength",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfoSegmentCache",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartOfNationalRoadsLength",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfoSegmentCache",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartOfNumberedRoadsLength",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfoSegmentCache",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartOfEuropeanRoadsLength",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfoSegmentCache");

            migrationBuilder.DropColumn(
                name: "PartOfNationalRoadsLength",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfoSegmentCache");

            migrationBuilder.DropColumn(
                name: "PartOfNumberedRoadsLength",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfoSegmentCache");
        }
    }
}
