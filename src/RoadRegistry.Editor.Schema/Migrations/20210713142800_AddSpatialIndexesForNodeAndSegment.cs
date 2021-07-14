using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddSpatialIndexesForNodeAndSegment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE SPATIAL INDEX [SIDX_RoadNode_Geometry] ON [RoadRegistryEditor].[RoadNode] ([Geometry])
                USING GEOMETRY_GRID
                WITH (
                    BOUNDING_BOX = (22000, 152500, 253000, 245000),
                    GRIDS = (
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
                CREATE SPATIAL INDEX [SIDX_RoadSegment_Geometry] ON [RoadRegistryEditor].[RoadSegment] ([Geometry])
                USING GEOMETRY_GRID
                WITH (
                    BOUNDING_BOX = (22000, 152500, 253000, 245000),
                    GRIDS = (
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX [SIDX_RoadNode_Geometry] ON [RoadRegistryEditor].[RoadNode]");
            migrationBuilder.Sql(@"DROP INDEX [SIDX_RoadSegment_Geometry] ON [RoadRegistryEditor].[RoadSegment]");
        }
    }
}
