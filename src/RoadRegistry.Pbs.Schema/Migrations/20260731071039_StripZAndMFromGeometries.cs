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
                migrationBuilder.Sql($@"
UPDATE [{schema}].[{table}]
SET [GEOMETRIE] = geometry::STGeomFromWKB([GEOMETRIE].STAsBinary(), [GEOMETRIE].STSrid)
WHERE [GEOMETRIE] IS NOT NULL;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible: the dropped Z/M ordinates cannot be reconstructed.
        }
    }
}
