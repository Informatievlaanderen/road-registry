using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentProducerSnapshot
{
    public partial class RemoveStreetNameCachePosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoadSegment_StreetNameCachePosition",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.DropColumn(
                name: "StreetNameCachePosition",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StreetNameCachePosition",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_StreetNameCachePosition",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                column: "StreetNameCachePosition")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
