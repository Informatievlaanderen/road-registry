using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "road");

            migrationBuilder.CreateTable(
                name: "AfgeleideWegsegmenten",
                schema: "road",
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
                    EUNUMMERS = table.Column<string>(type: "varchar(255)", nullable: true),
                    NWNUMMERS = table.Column<string>(type: "varchar(255)", nullable: true),
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                name: "OngelijkgrondseKruisingen",
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                name: "Wegknopen",
                schema: "road",
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
                name: "Wegsegmenten",
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
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
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_STATUS",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "STATUS");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_METHODE",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "METHODE");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_MORF",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "MORF");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_WEGCAT",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "WEGCAT");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_TOEGANG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "TOEGANG");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_VERHARDING",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "VERHARDING");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_AUTOHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "AUTOHEEN");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_AUTOTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "AUTOTERUG");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_FIETSHEEN",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "FIETSHEEN");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_FIETSTERUG",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "FIETSTERUG");

            migrationBuilder.CreateIndex(
                name: "IX_AfgeleideWegsegmenten_VOETGANGER",
                schema: "road",
                table: "AfgeleideWegsegmenten",
                column: "VOETGANGER");

            migrationBuilder.CreateIndex(
                name: "IX_EuropeseWegen_WS_OIDN",
                schema: "road",
                table: "EuropeseWegen",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_GelijkgrondseKruisingen_WS1_OIDN",
                schema: "road",
                table: "GelijkgrondseKruisingen",
                column: "WS1_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_GelijkgrondseKruisingen_WS2_OIDN",
                schema: "road",
                table: "GelijkgrondseKruisingen",
                column: "WS2_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_NationaleWegen_WS_OIDN",
                schema: "road",
                table: "NationaleWegen",
                column: "WS_OIDN")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_OngelijkgrondseKruisingen_BO_WS_OIDN",
                schema: "road",
                table: "OngelijkgrondseKruisingen",
                column: "BO_WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_OngelijkgrondseKruisingen_ON_WS_OIDN",
                schema: "road",
                table: "OngelijkgrondseKruisingen",
                column: "ON_WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentMorfologieAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentMorfologieAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentStraatnaamAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentStraatnaamAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentToegangAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentToegangAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentVerkeerstypeAutoAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentVerkeerstypeAutoAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentVerkeerstypeFietsAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentVerkeerstypeFietsAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentVerkeerstypeVoetgangerAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentVerkeerstypeVoetgangerAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentWegbeheerderAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentWegbeheerderAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentWegcategorieAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentWegcategorieAttributen",
                column: "WS_OIDN");

            migrationBuilder.CreateIndex(
                name: "IX_WegsegmentWegverhardingAttributen_WS_OIDN",
                schema: "road",
                table: "WegsegmentWegverhardingAttributen",
                column: "WS_OIDN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AfgeleideWegsegmenten",
                schema: "road");

            migrationBuilder.DropTable(
                name: "EuropeseWegen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "GelijkgrondseKruisingen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "NationaleWegen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "OngelijkgrondseKruisingen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "OrganisatieCache",
                schema: "road");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "road");

            migrationBuilder.DropTable(
                name: "StraatnaamCache",
                schema: "road");

            migrationBuilder.DropTable(
                name: "Wegknopen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "Wegsegmenten",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentMorfologieAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentStraatnaamAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentToegangAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentVerkeerstypeAutoAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentVerkeerstypeFietsAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentVerkeerstypeVoetgangerAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentWegbeheerderAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentWegcategorieAttributen",
                schema: "road");

            migrationBuilder.DropTable(
                name: "WegsegmentWegverhardingAttributen",
                schema: "road");
        }
    }
}
