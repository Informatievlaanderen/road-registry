using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentProducerSnapshot
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryRoadSegmentProducerSnapshotMeta");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryRoadSegmentProducerSnapshot");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryRoadSegmentProducerSnapshotMeta",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegment",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AccessRestrictionDutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessRestrictionId = table.Column<int>(type: "int", nullable: true),
                    BeginApplication = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeginOperator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeginOrganizationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeginOrganizationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeginRoadNodeId = table.Column<int>(type: "int", nullable: true),
                    BeginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoryDutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndRoadNodeId = table.Column<int>(type: "int", nullable: true),
                    Geometry = table.Column<Geometry>(type: "Geometry", nullable: true),
                    GeometryVersion = table.Column<int>(type: "int", nullable: true),
                    LeftSideMunicipalityId = table.Column<int>(type: "int", nullable: true),
                    LeftSideMunicipalityNisCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeftSideStreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeftSideStreetNameId = table.Column<int>(type: "int", nullable: true),
                    MaintainerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaintainerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MethodDutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MethodId = table.Column<int>(type: "int", nullable: true),
                    MorphologyDutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MorphologyId = table.Column<int>(type: "int", nullable: true),
                    RecordingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RightSideMunicipalityId = table.Column<int>(type: "int", nullable: true),
                    RightSideMunicipalityNisCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RightSideStreetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RightSideStreetNameId = table.Column<int>(type: "int", nullable: true),
                    RoadSegmentVersion = table.Column<int>(type: "int", nullable: true),
                    StatusDutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    StreetNameCachePosition = table.Column<long>(type: "bigint", nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    LastChangedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegment", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_StreetNameCachePosition",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                column: "StreetNameCachePosition")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryRoadSegmentProducerSnapshotMeta");

            migrationBuilder.DropTable(
                name: "RoadSegment",
                schema: "RoadRegistryRoadSegmentProducerSnapshot");
        }
    }
}
