using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.BackOffice.Schema.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryBackOfficeMeta");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryBackOffice");

            migrationBuilder.CreateTable(
                name: "RoadNodeBoundingBox",
                columns: table => new
                {
                    MinimumX = table.Column<double>(nullable: false),
                    MaximumX = table.Column<double>(nullable: false),
                    MinimumY = table.Column<double>(nullable: false),
                    MaximumY = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentBoundingBox",
                columns: table => new
                {
                    MinimumX = table.Column<double>(nullable: false),
                    MaximumX = table.Column<double>(nullable: false),
                    MinimumY = table.Column<double>(nullable: false),
                    MaximumY = table.Column<double>(nullable: false),
                    MinimumM = table.Column<double>(nullable: false),
                    MaximumM = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "GradeSeparatedJunction",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeSeparatedJunction", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DbaseRecord = table.Column<byte[]>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    SortableCode = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoadNetworkChange",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    When = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadNetworkChange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoadNetworkInfo",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false, defaultValue: 0),
                    CompletedImport = table.Column<bool>(nullable: false, defaultValue: false),
                    OrganizationCount = table.Column<int>(nullable: false, defaultValue: 0),
                    RoadNodeCount = table.Column<int>(nullable: false, defaultValue: 0),
                    TotalRoadNodeShapeLength = table.Column<int>(nullable: false),
                    RoadSegmentCount = table.Column<int>(nullable: false, defaultValue: 0),
                    TotalRoadSegmentShapeLength = table.Column<int>(nullable: false),
                    RoadSegmentEuropeanRoadAttributeCount = table.Column<int>(nullable: false, defaultValue: 0),
                    RoadSegmentNumberedRoadAttributeCount = table.Column<int>(nullable: false, defaultValue: 0),
                    RoadSegmentNationalRoadAttributeCount = table.Column<int>(nullable: false, defaultValue: 0),
                    RoadSegmentLaneAttributeCount = table.Column<int>(nullable: false, defaultValue: 0),
                    RoadSegmentWidthAttributeCount = table.Column<int>(nullable: false, defaultValue: 0),
                    RoadSegmentSurfaceAttributeCount = table.Column<int>(nullable: false, defaultValue: 0),
                    GradeSeparatedJunctionCount = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadNetworkInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoadNode",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(nullable: false),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false),
                    BoundingBox_MinimumX = table.Column<double>(nullable: true),
                    BoundingBox_MaximumX = table.Column<double>(nullable: true),
                    BoundingBox_MinimumY = table.Column<double>(nullable: true),
                    BoundingBox_MaximumY = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadNode", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegment",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(nullable: false),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false),
                    BoundingBox_MinimumX = table.Column<double>(nullable: true),
                    BoundingBox_MaximumX = table.Column<double>(nullable: true),
                    BoundingBox_MinimumY = table.Column<double>(nullable: true),
                    BoundingBox_MaximumY = table.Column<double>(nullable: true),
                    BoundingBox_MinimumM = table.Column<double>(nullable: true),
                    BoundingBox_MaximumM = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegment", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentEuropeanRoadAttribute",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentEuropeanRoadAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentLaneAttribute",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentLaneAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentNationalRoadAttribute",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentNationalRoadAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentNumberedRoadAttribute",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentNumberedRoadAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentSurfaceAttribute",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentSurfaceAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentWidthAttribute",
                schema: "RoadRegistryBackOffice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentWidthAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryBackOfficeMeta",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    DesiredState = table.Column<string>(nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Id",
                schema: "RoadRegistryBackOffice",
                table: "Organization",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadNetworkChange_Id",
                schema: "RoadRegistryBackOffice",
                table: "RoadNetworkChange",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadNetworkInfo_Id",
                schema: "RoadRegistryBackOffice",
                table: "RoadNetworkInfo",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentEuropeanRoadAttribute_RoadSegmentId",
                schema: "RoadRegistryBackOffice",
                table: "RoadSegmentEuropeanRoadAttribute",
                column: "RoadSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentLaneAttribute_RoadSegmentId",
                schema: "RoadRegistryBackOffice",
                table: "RoadSegmentLaneAttribute",
                column: "RoadSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentNationalRoadAttribute_RoadSegmentId",
                schema: "RoadRegistryBackOffice",
                table: "RoadSegmentNationalRoadAttribute",
                column: "RoadSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentNumberedRoadAttribute_RoadSegmentId",
                schema: "RoadRegistryBackOffice",
                table: "RoadSegmentNumberedRoadAttribute",
                column: "RoadSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentSurfaceAttribute_RoadSegmentId",
                schema: "RoadRegistryBackOffice",
                table: "RoadSegmentSurfaceAttribute",
                column: "RoadSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegmentWidthAttribute_RoadSegmentId",
                schema: "RoadRegistryBackOffice",
                table: "RoadSegmentWidthAttribute",
                column: "RoadSegmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadNodeBoundingBox");

            migrationBuilder.DropTable(
                name: "RoadSegmentBoundingBox");

            migrationBuilder.DropTable(
                name: "GradeSeparatedJunction",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "Organization",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadNetworkChange",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadNetworkInfo",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadNode",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadSegment",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadSegmentEuropeanRoadAttribute",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadSegmentLaneAttribute",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadSegmentNationalRoadAttribute",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadSegmentNumberedRoadAttribute",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadSegmentSurfaceAttribute",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "RoadSegmentWidthAttribute",
                schema: "RoadRegistryBackOffice");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryBackOfficeMeta");
        }
    }
}
