using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

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
                name: "ProjectionStates",
                schema: "RoadRegistryWmsMeta",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "wegsegmentDenorm",
                schema: "RoadRegistryWms",
                columns: table => new
                {
                    wegsegmentID = table.Column<int>(type: "int", nullable: false),
                    methode = table.Column<int>(type: "int", nullable: true),
                    beheerder = table.Column<string>(type: "varchar(18)", nullable: true),
                    begintijd = table.Column<string>(type: "varchar(100)", nullable: true),
                    beginoperator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    beginorganisatie = table.Column<string>(type: "varchar(18)", nullable: true),
                    beginapplicatie = table.Column<string>(type: "varchar(100)", nullable: true),
                    morfologie = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true),
                    categorie = table.Column<string>(type: "varchar(10)", nullable: true),
                    beginWegknoopID = table.Column<int>(type: "int", nullable: true),
                    eindWegknoopID = table.Column<int>(type: "int", nullable: true),
                    linksStraatnaamID = table.Column<int>(type: "int", nullable: true),
                    rechtsStraatnaamID = table.Column<int>(type: "int", nullable: true),
                    wegsegmentversie = table.Column<int>(type: "int", nullable: true),
                    geometrieversie = table.Column<int>(type: "int", nullable: true),
                    opnamedatum = table.Column<DateTime>(type: "datetime2", nullable: true),
                    toegangsbeperking = table.Column<int>(type: "int", nullable: true),
                    transactieID = table.Column<int>(type: "int", nullable: true),
                    linksGemeente = table.Column<int>(type: "int", nullable: true),
                    rechtsGemeente = table.Column<int>(type: "int", nullable: true),
                    linksGemeenteNisCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rechtsGemeenteNisCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lblCategorie = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblMethode = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblMorfologie = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblToegangsbeperking = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblStatus = table.Column<string>(type: "varchar(64)", nullable: true),
                    lblOrganisatie = table.Column<string>(type: "varchar(64)", nullable: true),
                    linksStraatnaam = table.Column<string>(type: "varchar(128)", nullable: true),
                    rechtsStraatnaam = table.Column<string>(type: "varchar(128)", nullable: true),
                    lblBeheerder = table.Column<string>(type: "varchar(64)", nullable: true),
                    geometrie2D = table.Column<Geometry>(type: "Geometry", nullable: true),
                    straatnaamCachePositie = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wegsegmentDenorm", x => x.wegsegmentID)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wegsegmentDenorm_straatnaamCachePositie",
                schema: "RoadRegistryWms",
                table: "wegsegmentDenorm",
                column: "straatnaamCachePositie")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryWmsMeta");

            migrationBuilder.DropTable(
                name: "wegsegmentDenorm",
                schema: "RoadRegistryWms");
        }
    }
}
