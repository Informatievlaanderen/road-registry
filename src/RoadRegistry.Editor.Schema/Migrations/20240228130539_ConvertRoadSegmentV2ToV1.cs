using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class ConvertRoadSegmentV2ToV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadSegment",
                schema: "RoadRegistryEditor");

            migrationBuilder.RenameTable(
                name: "RoadSegmentV2",
                schema: "RoadRegistryEditor",
                newName: "RoadSegment",
                newSchema: "RoadRegistryEditor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadSegmentV2",
                table: "RoadSegment",
                schema: "RoadRegistryEditor");

            migrationBuilder.AddPrimaryKey(
                    name: "PK_RoadSegment",
                    table: "RoadSegment",
                    column: "Id",
                    schema: "RoadRegistryEditor")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_LeftSideStreetNameId",
                    schema: "RoadRegistryEditor",
                    table: "RoadSegment");

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_MaintainerId",
                    schema: "RoadRegistryEditor",
                    table: "RoadSegment");

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_MethodId",
                    schema: "RoadRegistryEditor",
                    table: "RoadSegment");

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_RightSideStreetNameId",
                    schema: "RoadRegistryEditor",
                    table: "RoadSegment");

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_IsRemoved",
                    schema: "RoadRegistryEditor",
                    table: "RoadSegment");
            
            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_LeftSideStreetNameId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "LeftSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_MaintainerId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "MaintainerId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_MethodId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "MethodId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_RightSideStreetNameId",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "RightSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_IsRemoved",
                schema: "RoadRegistryEditor",
                table: "RoadSegment",
                column: "IsRemoved")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
