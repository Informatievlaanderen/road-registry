using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.WmsWfsV2.Schema.Migrations
{
    /// <inheritdoc />
    public partial class RoadSegmentDynamicAttributesJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "DynamicAttributes",
                schema: "road",
                table: "Wegsegmenten",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DynamicAttributes",
                schema: "road",
                table: "Wegsegmenten");

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
    }
}
