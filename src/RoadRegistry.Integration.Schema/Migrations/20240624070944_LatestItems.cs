using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class LatestItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grade_separated_junction_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    lower_road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    upper_road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    type_label = table.Column<string>(type: "text", nullable: true),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_separated_junction_latest_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    code = table.Column<string>(type: "text", nullable: false),
                    sortable_code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    ovo_code = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_latest_items", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "road_node_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    type_label = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    geometry = table.Column<Geometry>(type: "Geometry", nullable: false),
                    bounding_box_maximum_x = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_maximum_y = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_minimum_x = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_minimum_y = table.Column<double>(type: "double precision", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_node_latest_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_european_road_attribute_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_european_road_attribute_latest_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_lane_attribute_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    as_of_geometry_version = table.Column<int>(type: "integer", nullable: false),
                    from_position = table.Column<double>(type: "double precision", nullable: false),
                    to_position = table.Column<double>(type: "double precision", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    direction_id = table.Column<int>(type: "integer", nullable: false),
                    direction_label = table.Column<string>(type: "text", nullable: true),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_lane_attribute_latest_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_national_road_attribute_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_national_road_attribute_latest_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_numbered_road_attribute_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<string>(type: "text", nullable: true),
                    direction_id = table.Column<int>(type: "integer", nullable: false),
                    direction_label = table.Column<string>(type: "text", nullable: true),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_numbered_road_attribute_latest_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_surface_attribute_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    as_of_geometry_version = table.Column<int>(type: "integer", nullable: false),
                    from_position = table.Column<double>(type: "double precision", nullable: false),
                    to_position = table.Column<double>(type: "double precision", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    type_label = table.Column<string>(type: "text", nullable: true),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_surface_attribute_latest_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_width_attribute_latest_items",
                schema: "integration_road",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    as_of_geometry_version = table.Column<int>(type: "integer", nullable: false),
                    from_position = table.Column<double>(type: "double precision", nullable: false),
                    to_position = table.Column<double>(type: "double precision", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    width_label = table.Column<string>(type: "text", nullable: true),
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_width_attribute_latest_items", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_is_removed",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_lower_road_segment_id",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "lower_road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_type_id",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_type_label",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "type_label");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_upper_road_segment_id",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "upper_road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_version_timestamp",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_organization_latest_items_is_removed",
                schema: "integration_road",
                table: "organization_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_organization_latest_items_ovo_code",
                schema: "integration_road",
                table: "organization_latest_items",
                column: "ovo_code");

            migrationBuilder.CreateIndex(
                name: "IX_organization_latest_items_sortable_code",
                schema: "integration_road",
                table: "organization_latest_items",
                column: "sortable_code");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_latest_items_geometry",
                schema: "integration_road",
                table: "road_node_latest_items",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_latest_items_is_removed",
                schema: "integration_road",
                table: "road_node_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_latest_items_type_id",
                schema: "integration_road",
                table: "road_node_latest_items",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_latest_items_type_label",
                schema: "integration_road",
                table: "road_node_latest_items",
                column: "type_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_is_removed",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_number",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                column: "number");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_road_segm~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_latest_items_direction_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                column: "direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_latest_items_direction_label",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                column: "direction_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_latest_items_is_removed",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_latest_items_road_segment_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_latest_items_version_timestamp",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_is_removed",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_number",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                column: "number");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_road_segm~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_is_removed",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_number",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "number");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_road_segm~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_latest_items_is_removed",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_latest_items_road_segment_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_latest_items_type_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_latest_items_type_label",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                column: "type_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_latest_items_version_timesta~",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_latest_items_is_removed",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_latest_items_road_segment_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_latest_items_version_timestamp",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_latest_items_width",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                column: "width");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_latest_items_width_label",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                column: "width_label");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grade_separated_junction_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "organization_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_node_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_european_road_attribute_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_lane_attribute_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_national_road_attribute_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_numbered_road_attribute_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_surface_attribute_latest_items",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_width_attribute_latest_items",
                schema: "integration_road");
        }
    }
}
