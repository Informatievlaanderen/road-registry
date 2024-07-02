using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestampColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_node_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_node_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_node_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_node_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "organization_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "organization_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "organization_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "organization_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "version_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_node_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_node_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "road_node_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "road_node_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "organization_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "organization_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "organization_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "organization_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_versions");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_versions");

            migrationBuilder.DropColumn(
                name: "created_on_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items");

            migrationBuilder.DropColumn(
                name: "version_as_string",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items");
        }
    }
}
