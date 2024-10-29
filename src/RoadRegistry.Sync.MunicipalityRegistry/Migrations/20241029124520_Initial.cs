using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Sync.MunicipalityRegistry.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryMunicipalityEventConsumer");

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                schema: "RoadRegistryMunicipalityEventConsumer",
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
                schema: "RoadRegistryMunicipalityEventConsumer",
                table: "ProcessedMessages",
                column: "DateProcessed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedMessages",
                schema: "RoadRegistryMunicipalityEventConsumer");
        }
    }
}
