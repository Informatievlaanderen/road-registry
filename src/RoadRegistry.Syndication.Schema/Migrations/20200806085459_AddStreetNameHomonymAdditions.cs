using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class AddStreetNameHomonymAdditions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DutchHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrenchHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GermanHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DutchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "CONCAT(DutchName,'_',DutchHomonymAddition) PERSISTED");

            migrationBuilder.AddColumn<string>(
                name: "EnglishNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "CONCAT(EnglishName,'_',EnglishHomonymAddition) PERSISTED");

            migrationBuilder.AddColumn<string>(
                name: "FrenchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "CONCAT(FrenchName,'_',FrenchHomonymAddition) PERSISTED");

            migrationBuilder.AddColumn<string>(
                name: "GermanNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "CONCAT(GermanName,'_',GermanHomonymAddition) PERSISTED");

            migrationBuilder.AddColumn<string>(
                name: "NameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                nullable: true,
                computedColumnSql: "CONCAT(Name,'_',HomonymAddition) PERSISTED");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DutchHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "DutchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "EnglishHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "EnglishNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "FrenchHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "FrenchNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "GermanHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "GermanNameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "HomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "NameWithHomonymAddition",
                schema: "RoadRegistrySyndication",
                table: "StreetName");
        }
    }
}
