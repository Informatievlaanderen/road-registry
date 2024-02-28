using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Product.Schema.Migrations
{
    public partial class AddRoadSegmentsV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadSegmentV2",
                schema: "RoadRegistryProduct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    BoundingBox_MaximumM = table.Column<double>(type: "float", nullable: true),
                    BoundingBox_MaximumX = table.Column<double>(type: "float", nullable: true),
                    BoundingBox_MaximumY = table.Column<double>(type: "float", nullable: true),
                    BoundingBox_MinimumM = table.Column<double>(type: "float", nullable: true),
                    BoundingBox_MinimumX = table.Column<double>(type: "float", nullable: true),
                    BoundingBox_MinimumY = table.Column<double>(type: "float", nullable: true),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    EndNodeId = table.Column<int>(type: "int", nullable: false),
                    Geometry = table.Column<Geometry>(type: "Geometry", nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ShapeRecordContentLength = table.Column<int>(type: "int", nullable: false),
                    StartNodeId = table.Column<int>(type: "int", nullable: false),
                    LastEventHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    GeometryVersion = table.Column<int>(type: "int", nullable: false),
                    AccessRestrictionId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeftSideStreetNameId = table.Column<int>(type: "int", nullable: true),
                    MaintainerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaintainerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MethodId = table.Column<int>(type: "int", nullable: false),
                    MorphologyId = table.Column<int>(type: "int", nullable: false),
                    RightSideStreetNameId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    RecordingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BeginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BeginOrganizationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeginOrganizationName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentV2", x => x.Id)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentV2_IsRemoved",
                schema: "RoadRegistryProduct",
                table: "RoadSegmentV2",
                column: "IsRemoved")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentV2_LeftSideStreetNameId",
                schema: "RoadRegistryProduct",
                table: "RoadSegmentV2",
                column: "LeftSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentV2_MethodId",
                schema: "RoadRegistryProduct",
                table: "RoadSegmentV2",
                column: "MethodId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentV2_RightSideStreetNameId",
                schema: "RoadRegistryProduct",
                table: "RoadSegmentV2",
                column: "RightSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadSegmentV2",
                schema: "RoadRegistryProduct");
        }
    }
}
