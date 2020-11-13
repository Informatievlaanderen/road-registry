using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Product.Schema.Migrations
{
    public partial class AddRoadNetworkInfoSegmentCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadNetworkInfoSegmentCache",
                schema: "RoadRegistryProduct",
                columns: table => new
                {
                    RoadSegmentId = table.Column<int>(nullable: false),
                    ShapeLength = table.Column<int>(nullable: false),
                    SurfacesLength = table.Column<int>(nullable: false),
                    LanesLength = table.Column<int>(nullable: false),
                    WidthsLength = table.Column<int>(nullable: false),
                    PartOfEuropeanRoadsLength = table.Column<int>(nullable: false),
                    PartOfNationalRoadsLength = table.Column<int>(nullable: false),
                    PartOfNumberedRoadsLength = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadNetworkInfoSegmentCache", x => x.RoadSegmentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadNetworkInfoSegmentCache_RoadSegmentId",
                schema: "RoadRegistryProduct",
                table: "RoadNetworkInfoSegmentCache",
                column: "RoadSegmentId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadNetworkInfoSegmentCache",
                schema: "RoadRegistryProduct");
        }
    }
}
