using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RoadRegistry.Syndication.Schema.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistrySyndicationMeta");

            migrationBuilder.EnsureSchema(
                name: "RoadRegistrySyndication");

            migrationBuilder.CreateTable(
                name: "Municipality",
                schema: "RoadRegistrySyndication",
                columns: table => new
                {
                    MunicipalityId = table.Column<Guid>(nullable: false),
                    NisCode = table.Column<string>(nullable: true),
                    DutchName = table.Column<string>(nullable: true),
                    FrenchName = table.Column<string>(nullable: true),
                    GermanName = table.Column<string>(nullable: true),
                    EnglishName = table.Column<string>(nullable: true),
                    MunicipalityStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipality", x => x.MunicipalityId);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistrySyndicationMeta",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<long>(nullable: false),
                    DesiredState = table.Column<string>(nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Municipality_MunicipalityId",
                schema: "RoadRegistrySyndication",
                table: "Municipality",
                column: "MunicipalityId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Municipality",
                schema: "RoadRegistrySyndication");

            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistrySyndicationMeta");
        }
    }
}
