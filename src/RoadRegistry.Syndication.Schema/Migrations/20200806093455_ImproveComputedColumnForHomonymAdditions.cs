using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class ImproveComputedColumnForHomonymAdditions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "CONCAT(Name,'_',HomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "GermanNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "CONCAT(GermanName,'_',GermanHomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "FrenchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "CONCAT(FrenchName,'_',FrenchHomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "CONCAT(EnglishName,'_',EnglishHomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "DutchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComputedColumnSql: "CONCAT(DutchName,'_',DutchHomonymAddition) PERSISTED");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONCAT(Name,'_',HomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "GermanNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONCAT(GermanName,'_',GermanHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "FrenchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONCAT(FrenchName,'_',FrenchHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONCAT(EnglishName,'_',EnglishHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED");

            migrationBuilder.AlterColumn<string>(
                name: "DutchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "CONCAT(DutchName,'_',DutchHomonymAddition) PERSISTED",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED");
        }
    }
}
