using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class WmsProjectionAddSpatialIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_wegsegmentDenorm",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wegsegmentDenorm",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "wegsegmentID")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "wegsegmentmorfologie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "morfologie")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.Sql($@"
CREATE SPATIAL INDEX [SPATIAL_wegsegmentDeNorm2D0708] ON [RoadRegistryWms].[wegsegmentDeNorm]
(
    [geometrie2D]
)USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("[SPATIAL_wegsegmentDeNorm2D0708]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wegsegmentDenorm",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.DropIndex(
                name: "wegsegmentmorfologie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wegsegmentDenorm",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "wegsegmentID")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
