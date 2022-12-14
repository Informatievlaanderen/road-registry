using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentProducerSnapshot
{
    public partial class Origin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeginApplication",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "BeginOperator",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "BeginOrganizationId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "BeginTime",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.RenameColumn(
                name: "BeginOrganizationName",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                newName: "Origin_Organization");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Origin_Timestamp",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "datetimeoffset",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origin_Timestamp",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.RenameColumn(
                name: "Origin_Organization",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                newName: "BeginOrganizationName");

            migrationBuilder.AddColumn<string>(
                name: "BeginApplication",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeginOperator",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeginOrganizationId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BeginTime",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "datetime2",
                nullable: true);
        }
    }
}
