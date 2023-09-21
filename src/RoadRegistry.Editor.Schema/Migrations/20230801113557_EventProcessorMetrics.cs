using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class EventProcessorMetrics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RoadRegistryEditorMetrics");

            migrationBuilder.CreateTable(
                name: "EventProcessors",
                schema: "RoadRegistryEditorMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventProcessorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DbContext = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "EditorContext"),
                    FromPosition = table.Column<long>(type: "bigint", nullable: false),
                    ToPosition = table.Column<long>(type: "bigint", nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventProcessors", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventProcessors",
                schema: "RoadRegistryEditorMetrics");
        }
    }
}
