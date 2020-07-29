using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class CreateStreetNameSyndication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreetName",
                schema: "RoadRegistrySyndication",
                columns: table => new
                {
                    StreetNameId = table.Column<Guid>(nullable: false),
                    PersistentLocalId = table.Column<int>(nullable: true),
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DutchName = table.Column<string>(nullable: true),
                    FrenchName = table.Column<string>(nullable: true),
                    GermanName = table.Column<string>(nullable: true),
                    EnglishName = table.Column<string>(nullable: true),
                    StreetNameStatus = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetName", x => x.StreetNameId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreetName_StreetNameId",
                schema: "RoadRegistrySyndication",
                table: "StreetName",
                column: "StreetNameId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreetName",
                schema: "RoadRegistrySyndication");
        }
    }
}
