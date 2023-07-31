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
                    EventProcessorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FromPosition = table.Column<long>(type: "bigint", nullable: false),
                    ToPosition = table.Column<long>(type: "bigint", nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventProcessors", x => x.EventProcessorId)
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
