using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Jobs.Migrations
{
    /// <inheritdoc />
    public partial class AddDryRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DryRun",
                schema: "RoadRegistryJobs",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DryRun",
                schema: "RoadRegistryJobs",
                table: "Jobs");
        }
    }
}
