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
                defaultValue: new DateTimeOffset(new DateTime(2025, 3, 28, 10, 24, 52, 812, DateTimeKind.Unspecified).AddTicks(1768), new TimeSpan(0, 0, 0, 0, 0)));
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
