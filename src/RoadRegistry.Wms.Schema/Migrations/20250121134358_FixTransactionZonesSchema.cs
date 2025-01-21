using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Wms.Schema.Migrations
{
    /// <inheritdoc />
    public partial class FixTransactionZonesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OverlappendeBijwerkingszones",
                schema: "RoadRegistryWmsMeta",
                newName: "OverlappendeBijwerkingszones",
                newSchema: "RoadRegistryWms");

            migrationBuilder.RenameTable(
                name: "Bijwerkingszones",
                schema: "RoadRegistryWmsMeta",
                newName: "Bijwerkingszones",
                newSchema: "RoadRegistryWms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OverlappendeBijwerkingszones",
                schema: "RoadRegistryWms",
                newName: "OverlappendeBijwerkingszones",
                newSchema: "RoadRegistryWmsMeta");

            migrationBuilder.RenameTable(
                name: "Bijwerkingszones",
                schema: "RoadRegistryWms",
                newName: "Bijwerkingszones",
                newSchema: "RoadRegistryWmsMeta");
        }
    }
}
