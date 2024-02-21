using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Sync.StreetNameRegistry.Migrations.StreetNameEventProjection
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryStreetNameEvent");
            
            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryStreetNameEvent",
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
                name: "RenamedStreetName",
                schema: "RoadRegistryStreetNameEvent",
                columns: table => new
                {
                    StreetNameLocalId = table.Column<int>(type: "int", nullable: false),
                    DestinationStreetNameLocalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenamedStreetName", x => x.StreetNameLocalId);
                });
            
            migrationBuilder.CreateIndex(
                name: "IX_RenamedStreetName_StreetNameLocalId",
                schema: "RoadRegistryStreetNameEvent",
                table: "RenamedStreetName",
                column: "StreetNameLocalId")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryStreetNameEvent");

            migrationBuilder.DropTable(
                name: "RenamedStreetName",
                schema: "RoadRegistryStreetNameEvent");
        }
    }
}
