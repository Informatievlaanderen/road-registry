using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.StreetNameConsumer.Schema.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryStreetNameConsumer");

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryStreetNameConsumer",
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
                name: "StreetName",
                schema: "RoadRegistryStreetNameConsumer",
                columns: table => new
                {
                    StreetNameId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    MunicipalityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrenchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GermanName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DutchHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrenchHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GermanHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED"),
                    DutchNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED"),
                    FrenchNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED"),
                    GermanNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED"),
                    EnglishNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED"),
                    StreetNameStatus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetName", x => x.StreetNameId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_PersistentLocalId",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_StreetNameId",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                column: "StreetNameId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryStreetNameConsumer");

            migrationBuilder.DropTable(
                name: "StreetName",
                schema: "RoadRegistryStreetNameConsumer");
        }
    }
}
