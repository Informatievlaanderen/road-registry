using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    using BackOffice;

    /// <inheritdoc />
    public partial class RemoveOrganizationsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organization",
                schema: "RoadRegistryEditor");

            migrationBuilder.Sql($"DELETE FROM [{WellKnownSchemas.EditorMetaSchema}].[ProjectionStates] WHERE [Name] = 'roadregistry-editor-organization-projectionhost'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organization",
                schema: "RoadRegistryEditor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DbaseRecord = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    DbaseSchemaVersion = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "V1"),
                    SortableCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Id",
                schema: "RoadRegistryEditor",
                table: "Organization",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
