using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class WmsWfsV2SpatialIndexWegsegmenten : Migration
    {
        // [road].[Wegsegmenten] was the only table with a geometry column left without a spatial index.
        //
        // The index may already exist, because it can be created by hand ahead of this migration. Outside the migration's
        // transaction (suppressTransaction), so a build that fails halfway can be retried without holding a lock meanwhile.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[road].[Wegsegmenten]') AND name = N'SIDX_Wegsegmenten_GEOMETRIE')
    CREATE SPATIAL INDEX [SIDX_Wegsegmenten_GEOMETRIE] ON [road].[Wegsegmenten]
    (
        [GEOMETRIE]
    )USING  GEOMETRY_GRID
    WITH (BOUNDING_BOX =(522200, 653000, 758900, 744100), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
    CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[road].[Wegsegmenten]') AND name = N'SIDX_Wegsegmenten_GEOMETRIE')
    DROP INDEX [SIDX_Wegsegmenten_GEOMETRIE] ON [road].[Wegsegmenten];
", suppressTransaction: true);
        }
    }
}
