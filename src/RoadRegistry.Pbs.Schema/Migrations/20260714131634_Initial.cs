using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.Pbs.Schema.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryPbs");

            migrationBuilder.CreateTable(
                name: "AfgeleideWegsegmenten",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WS_TEMPID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    STATUS = table.Column<int>(type: "int", nullable: true),
                    LBLSTATUS = table.Column<string>(type: "varchar(64)", nullable: true),
                    METHODE = table.Column<int>(type: "int", nullable: true),
                    LBLMETHODE = table.Column<string>(type: "varchar(64)", nullable: true),
                    B_WK_OIDN = table.Column<int>(type: "int", nullable: true),
                    E_WK_OIDN = table.Column<int>(type: "int", nullable: true),
                    MORF = table.Column<int>(type: "int", nullable: true),
                    LBLMORF = table.Column<string>(type: "varchar(64)", nullable: true),
                    WEGCAT = table.Column<string>(type: "varchar(5)", nullable: true),
                    LBLWEGCAT = table.Column<string>(type: "varchar(64)", nullable: true),
                    LSTRNMID = table.Column<int>(type: "int", nullable: true),
                    RSTRNMID = table.Column<int>(type: "int", nullable: true),
                    LBEHEER = table.Column<string>(type: "varchar(18)", nullable: true),
                    RBEHEER = table.Column<string>(type: "varchar(18)", nullable: true),
                    TOEGANG = table.Column<int>(type: "int", nullable: true),
                    LBLTOEGANG = table.Column<string>(type: "varchar(64)", nullable: true),
                    VERHARDING = table.Column<int>(type: "int", nullable: true),
                    LBLVERHARD = table.Column<string>(type: "varchar(64)", nullable: true),
                    AUTOHEEN = table.Column<int>(type: "int", nullable: true),
                    AUTOTERUG = table.Column<int>(type: "int", nullable: true),
                    FIETSHEEN = table.Column<int>(type: "int", nullable: true),
                    FIETSTERUG = table.Column<int>(type: "int", nullable: true),
                    VOETGANGER = table.Column<int>(type: "int", nullable: true),
                    GEOMETRIE = table.Column<Geometry>(type: "Geometry", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfgeleideWegsegmenten", x => x.WS_TEMPID)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "EuropeseWegen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    EU_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    EUNUMMER = table.Column<string>(type: "varchar(4)", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EuropeseWegen", x => x.EU_OIDN)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "GelijkgrondseKruisingen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    GK_OIDN = table.Column<int>(type: "int", nullable: false),
                    WS1_OIDN = table.Column<int>(type: "int", nullable: false),
                    WS2_OIDN = table.Column<int>(type: "int", nullable: false),
                    GEOMETRIE = table.Column<Geometry>(type: "Geometry", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GelijkgrondseKruisingen", x => x.GK_OIDN)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "NationaleWegen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    NW_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    NWNUMMER = table.Column<string>(type: "varchar(8)", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationaleWegen", x => x.NW_OIDN)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "OngelijkgrondseKruisingCodelijstType",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    TYPE = table.Column<int>(type: "int", nullable: false),
                    LBLTYPE = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFTYPE = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OngelijkgrondseKruisingCodelijstType", x => x.TYPE);
                });

            migrationBuilder.CreateTable(
                name: "OngelijkgrondseKruisingen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    OK_OIDN = table.Column<int>(type: "int", nullable: false),
                    BO_WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    ON_WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    TYPE = table.Column<int>(type: "int", nullable: true),
                    LBLTYPE = table.Column<string>(type: "varchar(64)", nullable: true),
                    GEOMETRIE = table.Column<Geometry>(type: "Geometry", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OngelijkgrondseKruisingen", x => x.OK_OIDN)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "OrganisatieCache",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    OrganisatieId = table.Column<string>(type: "varchar(18)", nullable: false),
                    Naam = table.Column<string>(type: "varchar(64)", nullable: true),
                    OvoCode = table.Column<string>(type: "varchar(9)", nullable: true),
                    IsWegbeheerder = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisatieCache", x => x.OrganisatieId);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryPbs",
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
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "StraatnaamCache",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    StraatnaamId = table.Column<int>(type: "int", nullable: false),
                    Naam = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StraatnaamCache", x => x.StraatnaamId);
                });

            migrationBuilder.CreateTable(
                name: "WegknoopCodelijstType",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    TYPE = table.Column<int>(type: "int", nullable: false),
                    LBLTYPE = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFTYPE = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegknoopCodelijstType", x => x.TYPE);
                });

            migrationBuilder.CreateTable(
                name: "Wegknopen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WK_OIDN = table.Column<int>(type: "int", nullable: false),
                    TYPE = table.Column<int>(type: "int", nullable: true),
                    LBLTYPE = table.Column<string>(type: "varchar(64)", nullable: true),
                    GRENSKNOOP = table.Column<int>(type: "int", nullable: true),
                    GEOMETRIE = table.Column<Geometry>(type: "Geometry", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wegknopen", x => x.WK_OIDN)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstKant",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    KANT = table.Column<int>(type: "int", nullable: false),
                    LBLKANT = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFKANT = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstKant", x => x.KANT);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstMethode",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    METHODE = table.Column<int>(type: "int", nullable: false),
                    LBLMETHODE = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFMETHODE = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstMethode", x => x.METHODE);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstMorfologie",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    MORF = table.Column<int>(type: "int", nullable: false),
                    LBLMORF = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFMORF = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstMorfologie", x => x.MORF);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstRichting",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    RICHTING = table.Column<int>(type: "int", nullable: false),
                    LBLRICHT = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFRICHT = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstRichting", x => x.RICHTING);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstStatus",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    STATUS = table.Column<int>(type: "int", nullable: false),
                    LBLSTATUS = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFSTATUS = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstStatus", x => x.STATUS);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstToegang",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    TOEGANG = table.Column<int>(type: "int", nullable: false),
                    LBLTOEGANG = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFTOEGANG = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstToegang", x => x.TOEGANG);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstVerharding",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    VERHARDING = table.Column<int>(type: "int", nullable: false),
                    LBLVERHARD = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFVERHARD = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstVerharding", x => x.VERHARDING);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstWegbeheerder",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    BEHEER = table.Column<string>(type: "varchar(18)", nullable: false),
                    LBLBEHEER = table.Column<string>(type: "varchar(64)", nullable: true),
                    OVOCODE = table.Column<string>(type: "varchar(9)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstWegbeheerder", x => x.BEHEER);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentCodelijstWegcategorie",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WEGCAT = table.Column<string>(type: "varchar(5)", nullable: false),
                    LBLWEGCAT = table.Column<string>(type: "varchar(64)", nullable: true),
                    DEFWEGCAT = table.Column<string>(type: "varchar(254)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentCodelijstWegcategorie", x => x.WEGCAT);
                });

            migrationBuilder.CreateTable(
                name: "Wegsegmenten",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    STATUS = table.Column<int>(type: "int", nullable: true),
                    LBLSTATUS = table.Column<string>(type: "varchar(64)", nullable: true),
                    METHODE = table.Column<int>(type: "int", nullable: true),
                    LBLMETHODE = table.Column<string>(type: "varchar(64)", nullable: true),
                    B_WK_OIDN = table.Column<int>(type: "int", nullable: true),
                    E_WK_OIDN = table.Column<int>(type: "int", nullable: true),
                    GEOMETRIE = table.Column<Geometry>(type: "Geometry", nullable: true),
                    CREATIE = table.Column<string>(type: "varchar(15)", nullable: false),
                    VERSIE = table.Column<string>(type: "varchar(15)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wegsegmenten", x => x.WS_OIDN)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentMorfologieAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    MO_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    MORF = table.Column<int>(type: "int", nullable: true),
                    LBLMORF = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentMorfologieAttributen", x => x.MO_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentStraatnaamAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    SN_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    STRTNMID = table.Column<int>(type: "int", nullable: true),
                    KANT = table.Column<int>(type: "int", nullable: true),
                    LBLKANT = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentStraatnaamAttributen", x => x.SN_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentToegangAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    TO_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    TOEGANG = table.Column<int>(type: "int", nullable: true),
                    LBLTOEGANG = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentToegangAttributen", x => x.TO_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentVerkeerstypeAutoAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    AU_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    RICHTING = table.Column<int>(type: "int", nullable: true),
                    LBLRICHT = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentVerkeerstypeAutoAttributen", x => x.AU_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentVerkeerstypeFietsAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    FI_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    RICHTING = table.Column<int>(type: "int", nullable: true),
                    LBLRICHT = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentVerkeerstypeFietsAttributen", x => x.FI_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentVerkeerstypeVoetgangerAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    VO_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    RICHTING = table.Column<int>(type: "int", nullable: true),
                    LBLRICHT = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentVerkeerstypeVoetgangerAttributen", x => x.VO_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentWegbeheerderAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WB_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    BEHEER = table.Column<string>(type: "varchar(18)", nullable: true),
                    KANT = table.Column<int>(type: "int", nullable: true),
                    LBLKANT = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentWegbeheerderAttributen", x => x.WB_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentWegcategorieAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WC_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    WEGCAT = table.Column<string>(type: "varchar(5)", nullable: true),
                    LBLWEGCAT = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentWegcategorieAttributen", x => x.WC_OIDN);
                });

            migrationBuilder.CreateTable(
                name: "WegsegmentWegverhardingAttributen",
                schema: "RoadRegistryPbs",
                columns: table => new
                {
                    WV_OIDN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WS_OIDN = table.Column<int>(type: "int", nullable: false),
                    VERHARDING = table.Column<int>(type: "int", nullable: true),
                    LBLVERHARD = table.Column<string>(type: "varchar(64)", nullable: true),
                    VANPOS = table.Column<double>(type: "float", nullable: false),
                    TOTPOS = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WegsegmentWegverhardingAttributen", x => x.WV_OIDN);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "AfgeleideWegsegmenten",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_EuropeseWegen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "EuropeseWegen",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_GelijkgrondseKruisingen_WS1_OIDN",
                schema: "RoadRegistryPbs",
                table: "GelijkgrondseKruisingen",
                column: "WS1_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_GelijkgrondseKruisingen_WS2_OIDN",
                schema: "RoadRegistryPbs",
                table: "GelijkgrondseKruisingen",
                column: "WS2_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_NationaleWegen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "NationaleWegen",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_OngelijkgrondseKruisingen_BO_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "OngelijkgrondseKruisingen",
                column: "BO_WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_OngelijkgrondseKruisingen_ON_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "OngelijkgrondseKruisingen",
                column: "ON_WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentMorfologieAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentMorfologieAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentStraatnaamAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentStraatnaamAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentToegangAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentToegangAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentVerkeerstypeAutoAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentVerkeerstypeAutoAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentVerkeerstypeFietsAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentVerkeerstypeFietsAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentVerkeerstypeVoetgangerAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentVerkeerstypeVoetgangerAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentWegbeheerderAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentWegbeheerderAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentWegcategorieAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentWegcategorieAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentWegverhardingAttributen_WS_OIDN",
                schema: "RoadRegistryPbs",
                table: "WegsegmentWegverhardingAttributen",
                column: "WS_OIDN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AfgeleideWegsegmenten",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "EuropeseWegen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "GelijkgrondseKruisingen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "NationaleWegen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "OngelijkgrondseKruisingCodelijstType",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "OngelijkgrondseKruisingen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "OrganisatieCache",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "StraatnaamCache",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegknoopCodelijstType",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "Wegknopen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstKant",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstMethode",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstMorfologie",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstRichting",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstStatus",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstToegang",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstVerharding",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstWegbeheerder",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentCodelijstWegcategorie",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "Wegsegmenten",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentMorfologieAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentStraatnaamAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentToegangAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentVerkeerstypeAutoAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentVerkeerstypeFietsAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentVerkeerstypeVoetgangerAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentWegbeheerderAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentWegcategorieAttributen",
                schema: "RoadRegistryPbs");

            migrationBuilder.DropTable(
                name: "WegsegmentWegverhardingAttributen",
                schema: "RoadRegistryPbs");
        }
    }
}
