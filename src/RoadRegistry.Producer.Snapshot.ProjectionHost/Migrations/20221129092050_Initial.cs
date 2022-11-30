using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryProducerSnapshotMeta");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryProducerSnapshot");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryProducerSnapshotMeta",
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
                name: "RoadNode",
                schema: "RoadRegistryProducerSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Geometry = table.Column<Geometry>(type: "Geometry", nullable: false),
                    Origin_BeginTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Origin_Organization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastChangedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadNode", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryProducerSnapshotMeta");

            migrationBuilder.DropTable(
                name: "RoadNode",
                schema: "RoadRegistryProducerSnapshot");
        }
    }
}
