using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_road");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "integration_road",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "text", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "road_segment_latest_items",
                schema: "integration_road",
                columns: table => new
                {
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
                    begin_organization_id = table.Column<string>(type: "text", nullable: true),
                    begin_organization_name = table.Column<string>(type: "text", nullable: true),
                    version_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_on_timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_road_segment_latest_items", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_category_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_category_label",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "category_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_end_node_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "end_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_geometry",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_is_removed",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "is_removed");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_is_removed_status_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                columns: new[] { "is_removed", "status_id" });

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_left_side_street_name_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "left_side_street_name_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_maintainer_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "maintainer_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_method_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "method_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_method_label",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "method_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_morphology_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "morphology_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_morphology_label",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "morphology_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_right_side_street_name_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "right_side_street_name_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_start_node_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "start_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_status_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_status_label",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "status_label");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "integration_road");

            migrationBuilder.DropTable(
                name: "road_segment_latest_items",
                schema: "integration_road");
        }
    }
}
