using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RoadRegistry.Projections.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryOslo");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryShape");

            migrationBuilder.CreateTable(
                name: "RoadReferencePoints",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadReferencePoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryOslo",
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
                name: "RoadNode",
                schema: "RoadRegistryShape",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DbaseRecord = table.Column<byte[]>(nullable: true),
                    ShapeRecordContent = table.Column<byte[]>(nullable: true),
                    ShapeRecordContentLength = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadSegment", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadReferencePoints");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryOslo");

            migrationBuilder.DropTable(
                name: "RoadNode",
                schema: "RoadRegistryShape");

            migrationBuilder.DropTable(
                name: "RoadSegment",
                schema: "RoadRegistryShape");
        }
    }
}
