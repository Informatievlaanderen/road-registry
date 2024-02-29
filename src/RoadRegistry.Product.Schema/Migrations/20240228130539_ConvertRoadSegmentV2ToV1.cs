using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Product.Schema.Migrations
{
    public partial class ConvertRoadSegmentV2ToV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadSegment",
                schema: "RoadRegistryProduct");

            migrationBuilder.RenameTable(
                name: "RoadSegmentV2",
                schema: "RoadRegistryProduct",
                newName: "RoadSegment",
                newSchema: "RoadRegistryProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoadSegmentV2",
                table: "RoadSegment",
                schema: "RoadRegistryProduct");

            migrationBuilder.AddPrimaryKey(
                    name: "PK_RoadSegment",
                    table: "RoadSegment",
                    column: "Id",
                    schema: "RoadRegistryProduct")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_LeftSideStreetNameId",
                    schema: "RoadRegistryProduct",
                    table: "RoadSegment");

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_MaintainerId",
                    schema: "RoadRegistryProduct",
                    table: "RoadSegment");

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_MethodId",
                    schema: "RoadRegistryProduct",
                    table: "RoadSegment");

            migrationBuilder.DropIndex(
                    name: "IX_RoadSegmentV2_RightSideStreetNameId",
                    schema: "RoadRegistryProduct",
                    table: "RoadSegment");
            
            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_LeftSideStreetNameId",
                schema: "RoadRegistryProduct",
                table: "RoadSegment",
                column: "LeftSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_RightSideStreetNameId",
                schema: "RoadRegistryProduct",
                table: "RoadSegment",
                column: "RightSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                    name: "IX_RoadSegment_MaintainerId",
                    schema: "RoadRegistryProduct",
                    table: "RoadSegment",
                    column: "MaintainerId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
