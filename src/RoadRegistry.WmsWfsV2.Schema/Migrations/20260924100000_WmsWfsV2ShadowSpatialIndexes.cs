using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class WmsWfsV2ShadowSpatialIndexes : Migration
    {
        // Puts the spatial indexes on the shadow read model, right before the next migration promotes it.
        //
        // The shadow schema was created from the EF model (TempSchemaBootstrapper), and the model does not know about
        // the spatial indexes: those are raw SQL in WmsWfsV2LabelsSpatialIndexesTrafficType, written against [road].
        // The promotion moves the shadow's tables into [road] rather than copying their rows - the same table objects,
        // keeping whatever indexes they carry - so without this they would arrive without a spatial index and every
        // WMS bounding-box query would go back to scanning.
        //
        // Separate from the promotion, and outside its transaction, for two reasons: building four spatial indexes
        // over the whole read model is the expensive part of this change and has no business holding the swap's
        // transaction open, and doing it here means that the moment the swap commits the live schema already has
        // them - there is no window in which [road] is served without a spatial index.
        //
        // Does nothing wherever the shadow schema is not there (a fresh database, dev, test); the promotion leaves
        // that case alone too.

        private const string ShadowSchema = "roadTemp";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateSpatialIndex(migrationBuilder, "AfgeleideWegsegmenten");
            CreateSpatialIndex(migrationBuilder, "Wegknopen");
            CreateSpatialIndex(migrationBuilder, "GelijkgrondseKruisingen");
            CreateSpatialIndex(migrationBuilder, "OngelijkgrondseKruisingen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropSpatialIndex(migrationBuilder, "AfgeleideWegsegmenten");
            DropSpatialIndex(migrationBuilder, "Wegknopen");
            DropSpatialIndex(migrationBuilder, "GelijkgrondseKruisingen");
            DropSpatialIndex(migrationBuilder, "OngelijkgrondseKruisingen");
        }

        // The same index the live tables carry, down to the bounding box and the grid density: these tables are about
        // to become the live ones, so anything else would be a silent change of plan.
        //
        // Outside the migration's transaction (suppressTransaction), which is also why it has to be able to run twice:
        // a build that fails halfway leaves the migration unjournaled, and the retry must not trip over an index that
        // did get created.
        private static void CreateSpatialIndex(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE s.name = N'{ShadowSchema}' AND t.name = N'{table}')
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{ShadowSchema}].[{table}]') AND name = N'SIDX_{table}_GEOMETRIE')
    EXEC(N'
CREATE SPATIAL INDEX [SIDX_{table}_GEOMETRIE] ON [{ShadowSchema}].[{table}]
(
    [GEOMETRIE]
)USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);');
", suppressTransaction: true);
        }

        private static void DropSpatialIndex(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{ShadowSchema}].[{table}]') AND name = N'SIDX_{table}_GEOMETRIE')
    EXEC(N'DROP INDEX [SIDX_{table}_GEOMETRIE] ON [{ShadowSchema}].[{table}];');
", suppressTransaction: true);
        }
    }
}
