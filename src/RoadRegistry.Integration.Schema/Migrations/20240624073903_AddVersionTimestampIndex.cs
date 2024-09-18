using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionTimestampIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_on_timestamp",
                schema: "integration_road",
                table: "organization_latest_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "version_timestamp",
                schema: "integration_road",
                table: "organization_latest_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_directio~1",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "direction_label");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_direction~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "direction_id");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_version_t~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_version_t~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_latest_items_version_timestamp",
                schema: "integration_road",
                table: "road_segment_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_version_t~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_road_node_latest_items_version_timestamp",
                schema: "integration_road",
                table: "road_node_latest_items",
                column: "version_timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_organization_latest_items_version_timestamp",
                schema: "integration_road",
                table: "organization_latest_items",
                column: "version_timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_directio~1",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_direction~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_numbered_road_attribute_latest_items_version_t~",
                schema: "integration_road",
                table: "road_segment_numbered_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_national_road_attribute_latest_items_version_t~",
                schema: "integration_road",
                table: "road_segment_national_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_latest_items_version_timestamp",
                schema: "integration_road",
                table: "road_segment_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_segment_european_road_attribute_latest_items_version_t~",
                schema: "integration_road",
                table: "road_segment_european_road_attribute_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_road_node_latest_items_version_timestamp",
                schema: "integration_road",
                table: "road_node_latest_items");

            migrationBuilder.DropIndex(
                name: "IX_organization_latest_items_version_timestamp",
                schema: "integration_road",
                table: "organization_latest_items");

            migrationBuilder.DropColumn(
                name: "created_on_timestamp",
                schema: "integration_road",
                table: "organization_latest_items");

            migrationBuilder.DropColumn(
                name: "version_timestamp",
                schema: "integration_road",
                table: "organization_latest_items");
        }
    }
}
