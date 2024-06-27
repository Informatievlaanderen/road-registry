using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grade_separated_junction_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    lower_road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    upper_road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    type_label = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_separated_junction_versions", x => new { x.position, x.id });
                });

            migrationBuilder.CreateTable(
                name: "organization_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    ovo_code = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_versions", x => x.position);
                });

            migrationBuilder.CreateTable(
                name: "road_node_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    type_label = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    geometry = table.Column<Geometry>(type: "Geometry", nullable: false),
                    bounding_box_maximum_x = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_maximum_y = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_minimum_x = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_minimum_y = table.Column<double>(type: "double precision", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_node_versions", x => new { x.position, x.id });
                });

            migrationBuilder.CreateTable(
                name: "road_segment_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    bounding_box_maximum_m = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_maximum_x = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_maximum_y = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_minimum_m = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_minimum_x = table.Column<double>(type: "double precision", nullable: true),
                    bounding_box_minimum_y = table.Column<double>(type: "double precision", nullable: true),
                    end_node_id = table.Column<int>(type: "integer", nullable: false),
                    geometry = table.Column<Geometry>(type: "Geometry", nullable: false),
                    start_node_id = table.Column<int>(type: "integer", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    geometry_version = table.Column<int>(type: "integer", nullable: false),
                    access_restriction_id = table.Column<int>(type: "integer", nullable: false),
                    access_restriction_label = table.Column<string>(type: "text", nullable: true),
                    category_id = table.Column<string>(type: "text", nullable: true),
                    category_label = table.Column<string>(type: "text", nullable: true),
                    left_side_street_name_id = table.Column<int>(type: "integer", nullable: true),
                    maintainer_id = table.Column<string>(type: "text", nullable: true),
                    method_id = table.Column<int>(type: "integer", nullable: false),
                    method_label = table.Column<string>(type: "text", nullable: true),
                    morphology_id = table.Column<int>(type: "integer", nullable: false),
                    morphology_label = table.Column<string>(type: "text", nullable: true),
                    right_side_street_name_id = table.Column<int>(type: "integer", nullable: true),
                    status_id = table.Column<int>(type: "integer", nullable: false),
                    status_label = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_versions", x => new { x.position, x.id });
                });

            migrationBuilder.CreateTable(
                name: "road_segment_european_road_attribute_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_european_road_attribute_versions", x => new { x.position, x.id });
                    table.ForeignKey(
                        name: "FK_road_segment_european_road_attribute_versions_road_segment_~",
                        columns: x => new { x.position, x.road_segment_id },
                        principalSchema: "integration_road",
                        principalTable: "road_segment_versions",
                        principalColumns: new[] { "position", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_lane_attribute_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    as_of_geometry_version = table.Column<int>(type: "integer", nullable: false),
                    from_position = table.Column<double>(type: "double precision", nullable: false),
                    to_position = table.Column<double>(type: "double precision", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    direction_id = table.Column<int>(type: "integer", nullable: false),
                    direction_label = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_lane_attribute_versions", x => new { x.position, x.id });
                    table.ForeignKey(
                        name: "FK_road_segment_lane_attribute_versions_road_segment_versions_~",
                        columns: x => new { x.position, x.road_segment_id },
                        principalSchema: "integration_road",
                        principalTable: "road_segment_versions",
                        principalColumns: new[] { "position", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_national_road_attribute_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_national_road_attribute_versions", x => new { x.position, x.id });
                    table.ForeignKey(
                        name: "FK_road_segment_national_road_attribute_versions_road_segment_~",
                        columns: x => new { x.position, x.road_segment_id },
                        principalSchema: "integration_road",
                        principalTable: "road_segment_versions",
                        principalColumns: new[] { "position", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_numbered_road_attribute_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    number = table.Column<string>(type: "text", nullable: true),
                    direction_id = table.Column<int>(type: "integer", nullable: false),
                    direction_label = table.Column<string>(type: "text", nullable: true),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_numbered_road_attribute_versions", x => new { x.position, x.id });
                    table.ForeignKey(
                        name: "FK_road_segment_numbered_road_attribute_versions_road_segment_~",
                        columns: x => new { x.position, x.road_segment_id },
                        principalSchema: "integration_road",
                        principalTable: "road_segment_versions",
                        principalColumns: new[] { "position", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_surface_attribute_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    as_of_geometry_version = table.Column<int>(type: "integer", nullable: false),
                    from_position = table.Column<double>(type: "double precision", nullable: false),
                    to_position = table.Column<double>(type: "double precision", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    type_label = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_surface_attribute_versions", x => new { x.position, x.id });
                    table.ForeignKey(
                        name: "FK_road_segment_surface_attribute_versions_road_segment_versio~",
                        columns: x => new { x.position, x.road_segment_id },
                        principalSchema: "integration_road",
                        principalTable: "road_segment_versions",
                        principalColumns: new[] { "position", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_width_attribute_versions",
                schema: "integration_road",
                columns: table => new
                {
                    position = table.Column<long>(type: "bigint", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    road_segment_id = table.Column<int>(type: "integer", nullable: false),
                    as_of_geometry_version = table.Column<int>(type: "integer", nullable: false),
                    from_position = table.Column<double>(type: "double precision", nullable: false),
                    to_position = table.Column<double>(type: "double precision", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    width_label = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: true),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_width_attribute_versions", x => new { x.position, x.id });
                    table.ForeignKey(
                        name: "FK_road_segment_width_attribute_versions_road_segment_versions~",
                        columns: x => new { x.position, x.road_segment_id },
                        principalSchema: "integration_road",
                        principalTable: "road_segment_versions",
                        principalColumns: new[] { "position", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_id",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_is_removed",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_lower_road_segment_id",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "lower_road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_organization_id",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_organization_name",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_position",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_type_id",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_type_label",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "type_label");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_upper_road_segment_id",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "upper_road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_versions_version_timestamp",
                schema: "integration_road",
                table: "grade_separated_junction_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_organization_versions_code",
                schema: "integration_road",
                table: "organization_versions",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "IX_organization_versions_is_removed",
                schema: "integration_road",
                table: "organization_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_organization_versions_ovo_code",
                schema: "integration_road",
                table: "organization_versions",
                column: "ovo_code");

            migrationBuilder.CreateIndex(
                name: "IX_organization_versions_version_timestamp",
                schema: "integration_road",
                table: "organization_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_geometry",
                schema: "integration_road",
                table: "road_node_versions",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_id",
                schema: "integration_road",
                table: "road_node_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_is_removed",
                schema: "integration_road",
                table: "road_node_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_organization_id",
                schema: "integration_road",
                table: "road_node_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_organization_name",
                schema: "integration_road",
                table: "road_node_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_position",
                schema: "integration_road",
                table: "road_node_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_type_id",
                schema: "integration_road",
                table: "road_node_versions",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_type_label",
                schema: "integration_road",
                table: "road_node_versions",
                column: "type_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_versions_version_timestamp",
                schema: "integration_road",
                table: "road_node_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_id",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_is_removed",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_number",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "number");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_organization_~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_organization~1",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_position",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_position_road~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                columns: new[] { "position", "road_segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_road_segment_~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_versions_version_times~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_direction_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_direction_label",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "direction_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_is_removed",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_organization_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_organization_name",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_position",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_position_road_segment_~",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                columns: new[] { "position", "road_segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_road_segment_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_versions_version_timestamp",
                schema: "integration_road",
                table: "road_segment_lane_attribute_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_id",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_is_removed",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_number",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "number");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_organization_~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_organization~1",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_position",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_position_road~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                columns: new[] { "position", "road_segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_road_segment_~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_versions_version_times~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_direction_id",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_direction_lab~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "direction_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_id",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_is_removed",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_number",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "number");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_organization_~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_organization~1",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_position",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_position_road~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                columns: new[] { "position", "road_segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_road_segment_~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_versions_version_times~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_is_removed",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_organization_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_organization_name",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_position",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_position_road_segme~",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                columns: new[] { "position", "road_segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_road_segment_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_type_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_type_label",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "type_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_versions_version_timestamp",
                schema: "integration_road",
                table: "road_segment_surface_attribute_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_category_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_category_label",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "category_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_end_node_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "end_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_geometry",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_is_removed",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_is_removed_status_id",
                schema: "integration_road",
                table: "road_segment_versions",
                columns: new[] { "is_removed", "status_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_left_side_street_name_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "left_side_street_name_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_maintainer_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "maintainer_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_method_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "method_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_method_label",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "method_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_morphology_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "morphology_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_morphology_label",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "morphology_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_organization_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_organization_name",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_position",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_right_side_street_name_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "right_side_street_name_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_start_node_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "start_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_status_id",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_status_label",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "status_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_versions_version_timestamp",
                schema: "integration_road",
                table: "road_segment_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_is_removed",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_organization_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_organization_name",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_position",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_position_road_segment~",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                columns: new[] { "position", "road_segment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_road_segment_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "road_segment_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_version_timestamp",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_width",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "width");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_versions_width_label",
                schema: "integration_road",
                table: "road_segment_width_attribute_versions",
                column: "width_label");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grade_separated_junction_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "organization_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_node_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_european_road_attribute_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_lane_attribute_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_national_road_attribute_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_numbered_road_attribute_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_surface_attribute_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_width_attribute_versions",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_versions",
                schema: "integration_road");
        }
    }
}
