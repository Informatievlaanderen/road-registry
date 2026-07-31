using RoadRegistry.BackOffice;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Pbs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class StripZAndMFromGeometries : Migration
    {
        // Historic rows were projected from the domain geometry, which carries an M (measure/chainage) ordinate, so the
        // stored SQL Server geometries are measured/3D. Rebuild them as plain 2D: STAsBinary() emits OGC WKB (X/Y only)
        // and STGeomFromWKB reconstructs a 2D geometry (dropping Z and M). Rebuilding an already-2D geometry is a no-op,
        // so this is safe to run over every row.
        private static readonly string[] Tables =
        [
            "Wegsegmenten",
            "Wegknopen",
            "OngelijkgrondseKruisingen",
            "GelijkgrondseKruisingen",
            "AfgeleideWegsegmenten"
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = WellKnownSchemas.PbsSchema;
            foreach (var table in Tables)
            {
                // ISNULL on the SRID is required: SQL Server does not guarantee that the WHERE predicate is applied
                // before the SET expression is evaluated, so on tables holding NULL geometries (OngelijkgrondseKruisingen)
                // STSrid yields NULL and STGeomFromWKB fails with "parameter 2 is not allowed to be null". The fallback
                // only ever applies to rows the filter discards.
                migrationBuilder.Sql($@"
UPDATE [{schema}].[{table}]
SET [GEOMETRIE] = geometry::STGeomFromWKB([GEOMETRIE].STAsBinary(), ISNULL([GEOMETRIE].STSrid, 0))
WHERE [GEOMETRIE] IS NOT NULL AND ([GEOMETRIE].HasZ = 1 OR [GEOMETRIE].HasM = 1);");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible: the dropped Z/M ordinates cannot be reconstructed.
        }
    }
}
