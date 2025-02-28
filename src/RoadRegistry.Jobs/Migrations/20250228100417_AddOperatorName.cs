using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Jobs.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperatorName",
                schema: "RoadRegistryJobs",
                table: "Jobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorName",
                schema: "RoadRegistryJobs",
                table: "Jobs");
        }
    }
}
