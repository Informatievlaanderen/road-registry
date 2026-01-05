using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Extracts.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AddInwinningszone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inwinningszones",
                schema: "RoadRegistryExtracts",
                columns: table => new
                {
                    NisCode = table.Column<string>(type: "nvarchar(5)", nullable: false),
                    Contour = table.Column<Geometry>(type: "Geometry", nullable: false),
                    Completed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inwinningszones", x => x.NisCode)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inwinningszones_NisCode",
                schema: "RoadRegistryExtracts",
                table: "Inwinningszones",
                column: "NisCode");

            migrationBuilder.Sql($@"
CREATE SPATIAL INDEX [SPATIAL_Inwinningszones] ON [RoadRegistryExtracts].[Inwinningszones]
(
    [Contour]
) USING  GEOMETRY_GRID
WITH (BOUNDING_BOX =(22000, 152500, 253000, 245000), GRIDS =(LEVEL_1 = MEDIUM,LEVEL_2 = MEDIUM,LEVEL_3 = MEDIUM,LEVEL_4 = MEDIUM),
CELLS_PER_OBJECT = 16, PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inwinningszones",
                schema: "RoadRegistryExtracts");
        }
    }
}
