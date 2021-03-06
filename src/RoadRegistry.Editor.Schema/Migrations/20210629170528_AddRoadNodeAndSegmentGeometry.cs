﻿using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddRoadNodeAndSegmentGeometry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Geometry>(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                type: "Geometry",
                nullable: false);

            migrationBuilder.AddColumn<Geometry>(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadNode",
                type: "Geometry",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "Geometry",
                schema: "RoadRegistryEditor",
                table: "RoadNode");
        }
    }
}
