using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMunicipalitieGeometries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MunicipalityGeometry",
                schema: "RoadRegistryEditor");

            migrationBuilder.Sql($"""
                                  DELETE
                                  FROM [RoadRegistryEditorMeta].[ProjectionStates]
                                  WHERE [Name] = 'roadregistry-editor-municipality-projectionhost'
                                  """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MunicipalityGeometry",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    NisCode = table.Column<string>(type: "nchar(5)", fixedLength: true, maxLength: 5, nullable: false),
                    Geometry = table.Column<Geometry>(type: "Geometry", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipalityGeometry", x => x.NisCode)
                        .Annotation("SqlServer:Clustered", true);
                });
        }
    }
}
