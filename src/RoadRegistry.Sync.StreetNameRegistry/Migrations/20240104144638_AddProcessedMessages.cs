﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.StreetNameConsumer.Schema.Migrations
{
    public partial class AddProcessedMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                schema: "RoadRegistryStreetNameConsumer",
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
                schema: "RoadRegistryStreetNameConsumer",
                table: "ProcessedMessages",
                column: "DateProcessed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedMessages",
                schema: "RoadRegistryStreetNameConsumer");
        }
    }
}
