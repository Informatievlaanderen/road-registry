using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

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
                name: "Municipalities",
                schema: "RoadRegistryMunicipalityEventConsumer",
                columns: table => new
                {
                    MunicipalityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NisCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Geometry = table.Column<Geometry>(type: "Geometry", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.MunicipalityId)
                        .Annotation("SqlServer:Clustered", false);
                });

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
                name: "IX_Municipalities_NisCode",
                schema: "RoadRegistryMunicipalityEventConsumer",
                table: "Municipalities",
                column: "NisCode")
                .Annotation("SqlServer:Clustered", true);

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
                name: "Municipalities",
                schema: "RoadRegistryMunicipalityEventConsumer");

            migrationBuilder.DropTable(
                name: "ProcessedMessages",
                schema: "RoadRegistryMunicipalityEventConsumer");
        }
    }
}
