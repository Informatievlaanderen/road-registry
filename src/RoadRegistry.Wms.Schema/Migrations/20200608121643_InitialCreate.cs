using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace RoadRegistry.Wms.Schema.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryWmsMeta");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryWms");

            migrationBuilder.CreateTable(
                name: "wegsegmentDenorm",
                schema: "RoadRegistryWms",
                columns: table => new
                {
                    wegsegmentID = table.Column<int>(nullable: false),
                    methode = table.Column<int>(nullable: true),
                    beheerder = table.Column<string>(type: "varchar(18)", nullable: true),
                    begintijd = table.Column<string>(type: "varchar(100)", nullable: true),
                    beginoperator = table.Column<string>(nullable: true),
                    beginorganisatie = table.Column<string>(type: "varchar(18)", nullable: true),
                    beginapplicatie = table.Column<string>(type: "varchar(100)", nullable: true),
                    geometrie = table.Column<Geometry>(type: "Geometry", nullable: true),
                    morfologie = table.Column<int>(nullable: true),
                    status = table.Column<int>(nullable: true),
                    categorie = table.Column<string>(type: "varchar(10)", nullable: true),
                    beginWegknoopID = table.Column<int>(nullable: true),
                    eindWegknoopID = table.Column<int>(nullable: true),
                    linksStraatnaamID = table.Column<int>(nullable: true),
                    rechtsStraatnaamID = table.Column<int>(nullable: true),
                    wegsegmentversie = table.Column<int>(nullable: true),
                    geometrieversie = table.Column<int>(nullable: true),
                    opnamedatum = table.Column<DateTime>(nullable: true),
                    toegangsbeperking = table.Column<int>(nullable: true),
                    transactieID = table.Column<int>(nullable: true),
                    sourceID = table.Column<int>(nullable: true),
                    bronSourceID = table.Column<string>(type: "varchar(18)", nullable: true),
                    linksGemeente = table.Column<int>(nullable: true),
                    rechtsGemeente = table.Column<int>(nullable: true),
                    lblCategorie = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblMethode = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblMorfologie = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblToegangsbeperking = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblStatus = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblOrganisatie = table.Column<string>(type: "varchar(64)", nullable: true),
                    linksStraatnaam = table.Column<string>(type: "varchar(128)", nullable: true),
                    rechtsStraatnaam = table.Column<string>(type: "varchar(128)", nullable: true),
                    lblBeheerder = table.Column<string>(type: "varchar(64)", nullable: true),
                    geometrie2D = table.Column<Geometry>(type: "Geometry", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wegsegmentDenorm", x => x.wegsegmentID);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryWmsMeta",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    DesiredState = table.Column<string>(nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_wegsegmentID",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "wegsegmentID")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wegsegmentDenorm",
                schema: "RoadRegistryWms");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryWmsMeta");
        }
    }
}
