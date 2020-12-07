using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class ClusterAndFixLengthOfNisCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MunicipalityGeometry",
                schema: "RoadRegistryEditor",
                table: "MunicipalityGeometry");

            migrationBuilder.AlterColumn<string>(
                name: "NisCode",
                schema: "RoadRegistryEditor",
                table: "MunicipalityGeometry",
                fixedLength: true,
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MunicipalityGeometry",
                schema: "RoadRegistryEditor",
                table: "MunicipalityGeometry",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MunicipalityGeometry",
                schema: "RoadRegistryEditor",
                table: "MunicipalityGeometry");

            migrationBuilder.AlterColumn<string>(
                name: "NisCode",
                schema: "RoadRegistryEditor",
                table: "MunicipalityGeometry",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldFixedLength: true,
                oldMaxLength: 5);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MunicipalityGeometry",
                schema: "RoadRegistryEditor",
                table: "MunicipalityGeometry",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
