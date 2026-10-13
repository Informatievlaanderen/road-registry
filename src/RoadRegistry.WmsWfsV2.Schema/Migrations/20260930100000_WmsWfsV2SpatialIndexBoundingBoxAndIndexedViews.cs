using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class WmsWfsV2SpatialIndexBoundingBoxAndIndexedViews : Migration
    {
        private const string OldBoundingBox = "22000, 152500, 253000, 245000";
        private const string NewBoundingBox = "522200, 653000, 758900, 744100";

        private static readonly string[] SpatiallyIndexedTables =
        {
            "AfgeleideWegsegmenten",
            "Wegknopen",
            "GelijkgrondseKruisingen",
            "OngelijkgrondseKruisingen"
        };

        private static readonly (string Schema, string View, string Column)[] IndexedViews =
        {
            ("wms", "Wegsegmenten", "TempId"),
            ("wms", "Wegknopen", "WegknoopId"),
            ("wfs", "Wegsegmenten", "ObjectId"),
            ("wfs", "Wegknopen", "ObjectId")
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in SpatiallyIndexedTables)
            {
                DropSpatialIndex(migrationBuilder, table);
                CreateSpatialIndex(migrationBuilder, table, NewBoundingBox);
            }

            foreach (var (schema, view, column) in IndexedViews)
            {
                migrationBuilder.Sql($@"
CREATE UNIQUE CLUSTERED INDEX [IX_{view}_{column}] ON [{schema}].[{view}]
(
    [{column}]
);");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (schema, view, column) in IndexedViews)
            {
                migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{schema}].[{view}]') AND name = N'CIX_{view}_{column}')
    DROP INDEX [IX_{view}_{column}] ON [{schema}].[{view}];");
            }

            foreach (var table in SpatiallyIndexedTables)
            {
                DropSpatialIndex(migrationBuilder, table);
                CreateSpatialIndex(migrationBuilder, table, OldBoundingBox);
            }
        }

        private static void DropSpatialIndex(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[road].[{table}]') AND name = N'SIDX_{table}_GEOMETRIE')
    DROP INDEX [SIDX_{table}_GEOMETRIE] ON [road].[{table}];");
        }

        private static void CreateSpatialIndex(MigrationBuilder migrationBuilder, string table, string boundingBox)
        {
            migrationBuilder.Sql($@"
CREATE SPATIAL INDEX [SIDX_{table}_GEOMETRIE] ON [road].[{table}]
(
    [GEOMETRIE]
)USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =({boundingBox}), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);");
        }
    }
}
