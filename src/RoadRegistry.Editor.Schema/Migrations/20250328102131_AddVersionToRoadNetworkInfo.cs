using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionToRoadNetworkInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastChangedTimestamp",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfo",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(2025, 3, 28, 10, 21, 31, 66, DateTimeKind.Unspecified).AddTicks(1895), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastChangedTimestamp",
                schema: "RoadRegistryEditor",
                table: "RoadNetworkInfo");
        }
    }
}
