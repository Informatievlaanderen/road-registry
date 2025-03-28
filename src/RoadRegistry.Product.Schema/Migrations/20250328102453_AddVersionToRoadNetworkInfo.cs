using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Product.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionToRoadNetworkInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastChangedTimestamp",
                schema: "RoadRegistryProduct",
                table: "RoadNetworkInfo",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: DateTimeOffset.UtcNow);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastChangedTimestamp",
                schema: "RoadRegistryProduct",
                table: "RoadNetworkInfo");
        }
    }
}
