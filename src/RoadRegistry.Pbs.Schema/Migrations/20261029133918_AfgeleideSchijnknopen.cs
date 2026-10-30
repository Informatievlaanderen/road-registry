using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Pbs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class AfgeleideSchijnknopen : Migration
    {
        // The afgeleide schijnknopen (GAWR-7836): the road nodes the afgeleid product needs where splitting a road
        // segment on its dynamic attributes puts two AfgeleideWegsegmenten end to end. They are not road nodes of the
        // registry, so they get a table of their own instead of a flag in [WegknopenData], with the road segment they
        // belong to (WS_OIDN) and their place along it (VOLGNR) - internal columns, neither view shows them.
        //
        // [Wegknopen] (basisproduct) keeps reading [WegknopenData] alone and does not change. [AfgeleideWegknopen]
        // (afgeleid product) becomes the road nodes plus the afgeleide schijnknopen. The two sets cannot share an id:
        // the afgeleide ids start at 1.000.000.000.
        //
        // The table starts empty. The segments already projected get their schijnknopen from the one-time backfill
        // behind projections/roadnetworkchangespbsprojection/backfill-afgeleide-schijnknopen, which derives every
        // AfgeleideWegsegment again; until it ran, the afgeleid product is as it was.
        private const string DerivedProductView = @"
CREATE OR ALTER VIEW [RoadRegistryPbs].[AfgeleideWegknopen] AS
SELECT
     [WK_OIDN]
    ,[TYPE]
    ,[LBLTYPE]
    ,[GRENSKNOOP]
    ,[GEOMETRIE]
    ,[CREATIE]
    ,[VERSIE]
FROM [RoadRegistryPbs].[WegknopenData]
UNION ALL
SELECT
     [WK_OIDN]
    ,[TYPE]
    ,[LBLTYPE]
    ,[GRENSKNOOP]
    ,[GEOMETRIE]
    ,[CREATIE]
    ,[VERSIE]
FROM [RoadRegistryPbs].[AfgeleideSchijnknopen]";

        private const string PreviousDerivedProductView = @"
CREATE OR ALTER VIEW [RoadRegistryPbs].[AfgeleideWegknopen] AS
SELECT
     [WK_OIDN]
    ,[TYPE]
    ,[LBLTYPE]
    ,[GRENSKNOOP]
    ,[GEOMETRIE]
    ,[CREATIE]
    ,[VERSIE]
FROM [RoadRegistryPbs].[WegknopenData]";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AfgeleideSchijnknopen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WK_OIDN = table.Column<int>(type: "int", nullable: false),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    VOLGNR = table.Column<int>(type: "int", nullable: false),
                    TYPE = table.Column<int>(type: "int", nullable: true),
                    LBLTYPE = table.Column<string>(type: "varchar(64)", nullable: true),
                    GRENSKNOOP = table.Column<int>(type: "int", nullable: true),
                    GEOMETRIE = table.Column<Geometry>(type: "Geometry", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfgeleideSchijnknopen", x => x.WK_OIDN)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideSchijnknopen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "AfgeleideSchijnknopen",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.Sql(DerivedProductView);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(PreviousDerivedProductView);

            migrationBuilder.DropTable(
                name: "AfgeleideSchijnknopen",
                schema: "RoadRegistryPbs");
        }
    }
}
