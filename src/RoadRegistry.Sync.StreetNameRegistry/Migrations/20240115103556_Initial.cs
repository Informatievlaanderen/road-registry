using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Sync.StreetNameRegistry.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryStreetNameSnapshotConsumer");

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                schema: "RoadRegistryStreetNameSnapshotConsumer",
                columns: table => new
                {
                    IdempotenceKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateProcessed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.IdempotenceKey)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedMessages_DateProcessed",
                schema: "RoadRegistryStreetNameSnapshotConsumer",
                table: "ProcessedMessages",
                column: "DateProcessed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedMessages",
                schema: "RoadRegistryStreetNameSnapshotConsumer");
        }
    }
}
