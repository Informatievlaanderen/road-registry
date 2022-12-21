#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentSurfaceProducerSnapshot
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryRoadSegmentSurfaceProducerSnapshotMetaSchema");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotMetaSchema",
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
                name: "RoadSegmentSurface",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    RoadSegmentId = table.Column<int>(type: "int", nullable: false),
                    RoadSegmentGeometryVersion = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    TypeDutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromPosition = table.Column<double>(type: "float", nullable: false),
                    ToPosition = table.Column<double>(type: "float", nullable: false),
                    Origin_BeginTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Origin_OrganizationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origin_OrganizationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastChangedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentSurface", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotMetaSchema");

            migrationBuilder.DropTable(
                name: "RoadSegmentSurface",
                schema: "RoadRegistryRoadSegmentSurfaceProducerSnapshotSchema");
        }
    }
}
