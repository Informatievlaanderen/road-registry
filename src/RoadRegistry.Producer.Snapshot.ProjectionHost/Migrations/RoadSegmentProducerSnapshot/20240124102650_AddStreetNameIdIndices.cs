using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentProducerSnapshot
{
    public partial class AddStreetNameIdIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_LeftSideStreetNameId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                column: "LeftSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_RightSideStreetNameId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                column: "RightSideStreetNameId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoadSegment_LeftSideStreetNameId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.DropIndex(
                name: "IX_RoadSegment_RightSideStreetNameId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");
        }
    }
}
