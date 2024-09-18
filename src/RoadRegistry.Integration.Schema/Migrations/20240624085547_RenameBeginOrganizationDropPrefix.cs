using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class RenameBeginOrganizationDropPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_segment_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "road_node_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "road_node_latest_items",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "begin_organization_name",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                newName: "organization_name");

            migrationBuilder.RenameColumn(
                name: "begin_organization_id",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                newName: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_width_attribute_latest_items_organization_name",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_surface_attribute_latest_items_organization_na~",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_organiza~1",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_organizat~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_organiza~1",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_organizat~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_organization_name",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_lane_attribute_latest_items_organization_name",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_organiza~1",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_organizat~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_latest_items_organization_id",
                schema: "integration_road",
                table: "road_node_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_latest_items_organization_name",
                schema: "integration_road",
                table: "road_node_latest_items",
                column: "organization_name");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_organization_id",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_separated_junction_latest_items_organization_name",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                column: "organization_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_road_segment_width_attribute_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_width_attribute_latest_items_organization_name",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_surface_attribute_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_surface_attribute_latest_items_organization_na~",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_organiza~1",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_organizat~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_organiza~1",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_organizat~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_latest_items_organization_name",
                schema: "integration_road",
                table: "road_segment_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_lane_attribute_latest_items_organization_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_lane_attribute_latest_items_organization_name",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_organiza~1",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_organizat~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_node_latest_items_organization_id",
                schema: "integration_road",
                table: "road_node_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_node_latest_items_organization_name",
                schema: "integration_road",
                table: "road_node_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_grade_separated_junction_latest_items_organization_id",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_grade_separated_junction_latest_items_organization_name",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_segment_width_attribute_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_segment_surface_attribute_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_segment_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_segment_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_segment_lane_attribute_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "road_node_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "road_node_latest_items",
                newName: "begin_organization_id");

            migrationBuilder.RenameColumn(
                name: "organization_name",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                newName: "begin_organization_name");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                schema: "integration_road",
                table: "grade_separated_junction_latest_items",
                newName: "begin_organization_id");
        }
    }
}
