using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionZonesSpatialIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
CREATE SPATIAL INDEX [SPATIAL_Bijwerkingszones] ON [RoadRegistryWms].[Bijwerkingszones]
(
    [Contour]
) USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
");

            migrationBuilder.Sql($@"
CREATE SPATIAL INDEX [SPATIAL_OverlappendeBijwerkingszones] ON [RoadRegistryWms].[OverlappendeBijwerkingszones]
(
    [Contour]
) USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("[SPATIAL_Bijwerkingszones]");
            migrationBuilder.DropIndex("[SPATIAL_OverlappendeBijwerkingszones]");
        }
    }
}
