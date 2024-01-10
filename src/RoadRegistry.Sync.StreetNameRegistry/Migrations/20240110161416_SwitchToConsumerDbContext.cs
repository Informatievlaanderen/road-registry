using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.StreetNameConsumer.Schema.Migrations
{
    public partial class SwitchToConsumerDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectionStates",
                schema: "RoadRegistryStreetNameConsumer");

            migrationBuilder.DropColumn(
                name: "Offset",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Offset",
                schema: "RoadRegistryStreetNameConsumer",
                table: "StreetName",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ProjectionStates",
                schema: "RoadRegistryStreetNameConsumer",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DesiredState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DesiredStateChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionStates", x => x.Name)
                        .Annotation("SqlServer:Clustered", true);
                });
        }
    }
}
