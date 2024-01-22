using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class RemoveStreetName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreetName",
                schema: "RoadRegistrySyndication");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreetName",
                schema: "RoadRegistrySyndication",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DutchHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DutchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DutchNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(DutchName + COALESCE('_' + DutchHomonymAddition,''), DutchHomonymAddition) PERSISTED"),
                    EnglishHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(EnglishName + COALESCE('_' + EnglishHomonymAddition,''), EnglishHomonymAddition) PERSISTED"),
                    FrenchHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrenchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrenchNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(FrenchName + COALESCE('_' + FrenchHomonymAddition,''), FrenchHomonymAddition) PERSISTED"),
                    GermanHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GermanName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GermanNameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(GermanName + COALESCE('_' + GermanHomonymAddition,''), GermanHomonymAddition) PERSISTED"),
                    HomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MunicipalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameWithHomonymAddition = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "COALESCE(Name + COALESCE('_' + HomonymAddition,''), HomonymAddition) PERSISTED"),
                    NisCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    Position = table.Column<long>(type: "bigint", nullable: false),
                    StreetNameStatus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetName", x => x.StreetNameId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_PersistentLocalId",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                column: "PersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_Position",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                column: "Position");

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_StreetNameId",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                column: "StreetNameId")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
