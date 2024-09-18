using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Integration.Schema.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSortableCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_organization_latest_items_sortable_code",
                schema: "integration_road",
                table: "organization_latest_items");

            migrationBuilder.DropColumn(
                name: "sortable_code",
                schema: "integration_road",
                table: "organization_latest_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sortable_code",
                schema: "integration_road",
                table: "organization_latest_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_organization_latest_items_sortable_code",
                schema: "integration_road",
                table: "organization_latest_items",
                column: "sortable_code");
        }
    }
}
