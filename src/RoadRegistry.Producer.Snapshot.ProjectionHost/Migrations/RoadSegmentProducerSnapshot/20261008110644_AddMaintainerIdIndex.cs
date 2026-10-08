using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Migrations.RoadSegmentProducerSnapshot
{
    /// <inheritdoc />
    public partial class AddMaintainerIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MaintainerId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoadSegment_MaintainerId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                column: "MaintainerId")
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoadSegment_MaintainerId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment");

            migrationBuilder.AlterColumn<string>(
                name: "MaintainerId",
                schema: "RoadRegistryRoadSegmentProducerSnapshot",
                table: "RoadSegment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
