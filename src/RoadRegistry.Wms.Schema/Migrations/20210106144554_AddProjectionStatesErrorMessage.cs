﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class AddProjectionStatesErrorMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "RoadRegistryWmsMeta",
                table: "ProjectionStates",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "RoadRegistryWmsMeta",
                table: "ProjectionStates");
        }
    }
}
