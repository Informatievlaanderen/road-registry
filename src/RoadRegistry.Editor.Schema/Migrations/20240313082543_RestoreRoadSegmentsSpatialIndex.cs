using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class RestoreRoadSegmentsSpatialIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'SIDX_RoadSegment_Geometry' )
BEGIN
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
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX [SIDX_RoadSegment_Geometry] ON [RoadRegistryEditor].[RoadSegment]");
        }
    }
}
