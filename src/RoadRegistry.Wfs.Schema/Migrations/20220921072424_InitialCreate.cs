using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Wfs.Schema.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryWfs");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistryWfsMeta");

            migrationBuilder.CreateTable(
                name: "OngelijkgrondseKruising",
                schema: "RoadRegistryWfs",
                columns: table => new
                {
                    objectId = table.Column<int>(type: "int", nullable: false),
                    versieId = table.Column<string>(type: "varchar(100)", nullable: true),
                    type = table.Column<string>(type: "varchar(255)", nullable: true),
                    onderliggendWegsegment = table.Column<int>(type: "int", nullable: true),
                    bovenliggendWegsegment = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OngelijkgrondseKruising", x => x.objectId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryWfsMeta",
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
                name: "Wegknoop",
                schema: "RoadRegistryWfs",
                columns: table => new
                {
                    objectId = table.Column<int>(type: "int", nullable: false),
                    versieId = table.Column<string>(type: "varchar(100)", nullable: true),
                    type = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wegknoop", x => x.objectId)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Wegsegment",
                schema: "RoadRegistryWfs",
                columns: table => new
                {
                    objectId = table.Column<int>(type: "int", nullable: false),
                    versieId = table.Column<string>(type: "varchar(100)", nullable: true),
                    middellijnGeometrie = table.Column<Geometry>(type: "Geometry", nullable: true),
                    beginknoopObjectId = table.Column<int>(type: "int", nullable: true),
                    eindknoopObjectId = table.Column<int>(type: "int", nullable: true),
                    wegsegmentstatus = table.Column<string>(type: "varchar(64)", nullable: true),
                    morfologischeWegklasse = table.Column<string>(type: "varchar(64)", nullable: true),
                    wegcategorie = table.Column<string>(type: "varchar(64)", nullable: true),
                    linkerstraatnaamObjectId = table.Column<int>(type: "int", nullable: true),
                    linkerstraatnaam = table.Column<string>(type: "varchar(128)", nullable: true),
                    rechterstraatnaamObjectId = table.Column<int>(type: "int", nullable: true),
                    rechterstraatnaam = table.Column<string>(type: "varchar(128)", nullable: true),
                    toegangsbeperking = table.Column<int>(type: "int", nullable: true),
                    labelToegangsbeperking = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    methodeWegsegmentgeometrie = table.Column<string>(type: "varchar(64)", nullable: true),
                    wegbeheerder = table.Column<string>(type: "varchar(18)", nullable: true),
                    labelWegbeheerder = table.Column<string>(type: "varchar(64)", nullable: true),
                    StreetNameCachePosition = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wegsegment", x => x.objectId)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OngelijkgrondseKruising",
                schema: "RoadRegistryWfs");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryWfsMeta");

            migrationBuilder.DropTable(
                name: "Wegknoop",
                schema: "RoadRegistryWfs");

            migrationBuilder.DropTable(
                name: "Wegsegment",
                schema: "RoadRegistryWfs");
        }
    }
}
