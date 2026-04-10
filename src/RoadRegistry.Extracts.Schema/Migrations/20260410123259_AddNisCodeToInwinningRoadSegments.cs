using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddNisCodeToInwinningRoadSegments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InwinningRoadSegments",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "NisCode",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InwinningRoadSegments",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments",
                column: "Id")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_InwinningRoadSegments_NisCode",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments",
                column: "NisCode");

            migrationBuilder.CreateIndex(
                name: "IX_InwinningRoadSegments_RoadSegmentId",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments",
                column: "RoadSegmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InwinningRoadSegments",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments");

            migrationBuilder.DropIndex(
                name: "IX_InwinningRoadSegments_NisCode",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments");

            migrationBuilder.DropIndex(
                name: "IX_InwinningRoadSegments_RoadSegmentId",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments");

            migrationBuilder.DropColumn(
                name: "NisCode",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InwinningRoadSegments",
                schema: "RoadRegistryExtracts",
                table: "InwinningRoadSegments",
                column: "RoadSegmentId")
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
