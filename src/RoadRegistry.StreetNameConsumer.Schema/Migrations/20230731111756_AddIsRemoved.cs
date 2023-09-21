using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.StreetNameConsumer.Schema.Migrations
{
    public partial class AddIsRemoved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StreetNameStatus",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");

            migrationBuilder.AlterColumn<int>(
                name: "StreetNameStatus",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
