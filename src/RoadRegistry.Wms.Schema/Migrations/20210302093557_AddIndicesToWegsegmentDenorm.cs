using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class AddIndicesToWegsegmentDenorm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX [SPATIAL_wegsegmentDeNorm2D0708] ON [RoadRegistryWms].[wegsegmentDeNorm]([geometrie2D])
                USING  GEOMETRY_GRID
                WITH (
                    BOUNDING_BOX =(22000, 152500, 253000, 245000),
                    GRIDS =(
                        LEVEL_1 = MEDIUM,
                        LEVEL_2 = MEDIUM,
                        LEVEL_3 = MEDIUM,
                        LEVEL_4 = MEDIUM),
                    CELLS_PER_OBJECT = 16,
                    PAD_INDEX = OFF,
                    STATISTICS_NORECOMPUTE = OFF,
                    SORT_IN_TEMPDB = OFF,
                    DROP_EXISTING = OFF,
                    ONLINE = OFF,
                    ALLOW_ROW_LOCKS = ON,
                    ALLOW_PAGE_LOCKS = ON)
                GO");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [wegsegmentmorfologie] ON [RoadRegistryWms].[wegsegmentDeNorm]([morfologie] ASC)
                WITH (
                    PAD_INDEX = OFF,
                    STATISTICS_NORECOMPUTE = OFF,
                    SORT_IN_TEMPDB = OFF,
                    DROP_EXISTING = OFF,
                    ONLINE = OFF,
                    ALLOW_ROW_LOCKS = ON,
                    ALLOW_PAGE_LOCKS = ON)
                GO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX [SPATIAL_wegsegmentDeNorm2D0708] ON [RoadRegistryWms].[wegsegmentDeNorm]");
            migrationBuilder.Sql(@"DROP INDEX [wegsegmentmorfologie] ON [RoadRegistryWms].[wegsegmentDeNorm]");
        }
    }
}
