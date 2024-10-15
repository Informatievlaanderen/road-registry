using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationKboNumberAndIsMaintainer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_maintainer",
                schema: "integration_road",
                table: "organization_versions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "kbo_number",
                schema: "integration_road",
                table: "organization_versions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_maintainer",
                schema: "integration_road",
                table: "organization_latest_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "kbo_number",
                schema: "integration_road",
                table: "organization_latest_items",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_versions_is_maintainer",
                schema: "integration_road",
                table: "organization_versions",
                column: "is_maintainer");

            migrationBuilder.CreateIndex(
                name: "IX_organization_latest_items_is_maintainer",
                schema: "integration_road",
                table: "organization_latest_items",
                column: "is_maintainer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organization_versions_is_maintainer",
                schema: "integration_road",
                table: "organization_versions");

            migrationBuilder.DropIndex(
                name: "IX_organization_latest_items_is_maintainer",
                schema: "integration_road",
                table: "organization_latest_items");

            migrationBuilder.DropColumn(
                name: "is_maintainer",
                schema: "integration_road",
                table: "organization_versions");

            migrationBuilder.DropColumn(
                name: "kbo_number",
                schema: "integration_road",
                table: "organization_versions");

            migrationBuilder.DropColumn(
                name: "is_maintainer",
                schema: "integration_road",
                table: "organization_latest_items");

            migrationBuilder.DropColumn(
                name: "kbo_number",
                schema: "integration_road",
                table: "organization_latest_items");
        }
    }
}
