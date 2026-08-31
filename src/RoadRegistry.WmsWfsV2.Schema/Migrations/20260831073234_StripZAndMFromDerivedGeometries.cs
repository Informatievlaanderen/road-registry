using RoadRegistry.BackOffice;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class StripZAndMFromDerivedGeometries : Migration
    {
        // A second pass of 20260731070309_StripZAndMFromGeometries, needed because the flattened rows picked up a
        // Z and an M ordinate again after that migration ran. The road segment geometry is only forced to 2D on its
        // way in from an event; every event after the add re-derived the flattened rows from the segment geometry as it
        // came back out of SQL Server, and EF materializes a geometry column with SqlServerBytesReader, which always
        // builds coordinate sequences declaring a Z and an M ordinate - so writing one straight back marks the stored
        // geometry as 3D/measured. The flattener now normalizes what it is handed; this repairs the rows already written.
        //
        // STAsBinary() emits OGC WKB (X/Y only) and STGeomFromWKB reconstructs a 2D geometry, dropping Z and M.
        // Rebuilding an already-2D geometry is a no-op, so this is safe to run over every row.
        private const string Table = "AfgeleideWegsegmenten";
        private const int BatchSize = 50000;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = WellKnownSchemas.WmsWfsV2Schema;

            // Only the flattened table can carry the regression, and after a rebuild that is every one of its rows, so
            // the update walks the identity key in batches, outside the migration transaction, rather than holding one
            // table-sized transaction. Advancing over the key range - rather than looping while rows still match - also
            // bounds the loop no matter what a single row does. A run that is cut short simply repairs fewer rows: the
            // rewrite is idempotent and only touches rows that still carry a Z or an M.
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
