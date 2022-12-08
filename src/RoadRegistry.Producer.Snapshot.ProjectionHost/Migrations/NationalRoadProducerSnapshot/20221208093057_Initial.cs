#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.NationalRoadProducerSnapshot
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "NationalRoadProducerSnapshotSchema");

            migrationBuilder.EnsureSchema(
                name: "NationalRoadProducerSnapshotMetaSchema");

            migrationBuilder.CreateTable(
                name: "NationalRoad",
                schema: "NationalRoadProducerSnapshotSchema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    RoadSegmentId = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origin_BeginTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Origin_Organization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastChangedTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationalRoad", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "NationalRoadProducerSnapshotMetaSchema",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NationalRoad",
                schema: "NationalRoadProducerSnapshotSchema");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "NationalRoadProducerSnapshotMetaSchema");
        }
    }
}
