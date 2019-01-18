using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.BackOffice.Schema.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryProjectionMetaData");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryShape");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryProjectionMetaData",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "GradeSeparatedJunction",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeSeparatedJunction", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    SortableCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoadNetworkInfo",
                schema: "RoadRegistryShape",
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
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    Envelope_MinimumX = table.Column<double>(nullable: false),
                    Envelope_MaximumX = table.Column<double>(nullable: false),
                    Envelope_MinimumY = table.Column<double>(nullable: false),
                    Envelope_MaximumY = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadNode", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegment",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    Envelope_MinimumX = table.Column<double>(nullable: false),
                    Envelope_MaximumX = table.Column<double>(nullable: false),
                    Envelope_MinimumY = table.Column<double>(nullable: false),
                    Envelope_MaximumY = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegment", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentEuropeanRoadAttribute",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentEuropeanRoadAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentLaneAttribute",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentLaneAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentNationalRoadAttribute",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentNationalRoadAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentNumberedRoadAttribute",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentNumberedRoadAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentSurfaceAttribute",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentSurfaceAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RoadSegmentWidthAttribute",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    RoadSegmentId = table.Column<int>(nullable: false),
                    DbaseRecord = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegmentWidthAttribute", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Id",
                schema: "RoadRegistryShape",
                table: "Organization",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadNetworkInfo_Id",
                schema: "RoadRegistryShape",
                table: "RoadNetworkInfo",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryProjectionMetaData");

            migrationBuilder.DropTable(
                name: "GradeSeparatedJunction",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "Organization",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadNetworkInfo",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadNode",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegment",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegmentEuropeanRoadAttribute",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegmentLaneAttribute",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegmentNationalRoadAttribute",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegmentNumberedRoadAttribute",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegmentSurfaceAttribute",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegmentWidthAttribute",
                schema: "RoadRegistryShape");
        }
    }
}
