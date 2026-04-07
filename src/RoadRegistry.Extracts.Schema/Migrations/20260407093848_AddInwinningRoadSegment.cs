using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddInwinningRoadSegment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InwinningRoadSegments",
                schema: "RoadRegistryExtracts",
                columns: table => new
                {
                    RoadSegmentId = table.Column<int>(type: "int", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InwinningRoadSegments", x => x.RoadSegmentId)
                        .Annotation("SqlServer:Clustered", true);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InwinningRoadSegments",
                schema: "RoadRegistryExtracts");
        }
    }
}
