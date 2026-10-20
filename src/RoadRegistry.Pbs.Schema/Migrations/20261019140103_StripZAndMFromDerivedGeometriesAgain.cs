using RoadRegistry.BackOffice;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Pbs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class StripZAndMFromDerivedGeometriesAgain : Migration
    {
        // A third pass over the flattened rows. 20260731071039 cleaned them once and 20260831073246 a second time,
        // after the geometry handed to the flattener turned out to come back out of SQL Server with a Z and an M
        // declared. Normalizing that input was not enough: the flattener cuts a sub-line per attribute range, and an
        // interpolated cut point is a CoordinateZ in NTS - so every sub-line of a segment whose attributes do not all
        // run end to end declared a Z again, and SqlServerBytesWriter stored the row as 3D. The flattener now forces
        // the cut sub-line back to 2D; this repairs the rows already written.
        //
        // STAsBinary() emits OGC WKB (X/Y only) and STGeomFromWKB reconstructs a 2D geometry, dropping Z and M.
        // Rebuilding an already-2D geometry is a no-op, so this is safe to run over every row.
        private const string Table = "AfgeleideWegsegmenten";
        private const int BatchSize = 50000;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = WellKnownSchemas.PbsSchema;

            // Only the flattened table can carry this regression, and the update walks the identity key in batches,
            // outside the migration transaction, rather than holding one table-sized transaction. Advancing over the
            // key range - rather than looping while rows still match - also bounds the loop no matter what a single
            // row does. A run that is cut short simply repairs fewer rows: the rewrite is idempotent and only touches
            // rows that still carry a Z or an M.
            //
            // ISNULL on the SRID is required: SQL Server does not guarantee that the WHERE predicate is applied before
            // the SET expression is evaluated, so a NULL geometry would make STGeomFromWKB fail with "parameter 2 is
            // not allowed to be null". The fallback only ever applies to rows the filter discards.
            migrationBuilder.Sql($@"
DECLARE @Id int = 0;
DECLARE @MaxId int = (SELECT ISNULL(MAX([WS_TEMPID]), 0) FROM [{schema}].[{Table}]);
WHILE @Id <= @MaxId
BEGIN
    UPDATE [{schema}].[{Table}]
    SET [GEOMETRIE] = geometry::STGeomFromWKB([GEOMETRIE].STAsBinary(), ISNULL([GEOMETRIE].STSrid, 0))
    WHERE [WS_TEMPID] > @Id AND [WS_TEMPID] <= @Id + {BatchSize}
      AND [GEOMETRIE] IS NOT NULL AND ([GEOMETRIE].HasZ = 1 OR [GEOMETRIE].HasM = 1);

    SET @Id = @Id + {BatchSize};
END;", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible: the dropped Z/M ordinates cannot be reconstructed.
        }
    }
}
