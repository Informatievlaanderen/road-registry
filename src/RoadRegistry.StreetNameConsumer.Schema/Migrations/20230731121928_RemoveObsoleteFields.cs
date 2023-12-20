using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.StreetNameConsumer.Schema.Migrations
{
    public partial class RemoveObsoleteFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameWithHomonymAddition",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "HomonymAddition",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "MunicipalityId",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");

            migrationBuilder.AlterColumn<string>(
                name: "StreetNameStatus",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "PersistentLocalId",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NisCode",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_StreetNameStatus",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                column: "StreetNameStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StreetName_StreetNameStatus",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");

            migrationBuilder.AlterColumn<string>(
                name: "StreetNameStatus",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersistentLocalId",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "NisCode",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomonymAddition",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityId",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameWithHomonymAddition",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql: "COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED");
        }
    }
}
