using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExtractRequestOverlap",
                schema: "RoadRegistryEditor",
                table: "ExtractRequestOverlap");

            migrationBuilder.AddPrimaryKey(
                    name: "PK_ExtractRequestOverlap",
                    schema: "RoadRegistryEditor",
                    table: "ExtractRequestOverlap",
                    column: "Id")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtractRequestOverlap_DownloadId1",
                schema: "RoadRegistryEditor",
                table: "ExtractRequestOverlap",
                column: "DownloadId1");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractRequestOverlap_DownloadId2",
                schema: "RoadRegistryEditor",
                table: "ExtractRequestOverlap",
                column: "DownloadId2");

            migrationBuilder.Sql(
                @"CREATE SPATIAL INDEX [SPATIAL_RoadRegistryEditorExtractRequestOverlap_Contour] ON [RoadRegistryEditor].[ExtractRequestOverlap] ([Contour])
                    USING  GEOMETRY_GRID
	                WITH (
		                BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
		                GRIDS =(
			                LEVEL_1 = MEDIUM,
			                LEVEL_2 = MEDIUM,
			                LEVEL_3 = MEDIUM,
			                LEVEL_4 = MEDIUM),
	                CELLS_PER_OBJECT = 5)
	                GO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX [SPATIAL_RoadRegistryEditorExtractRequestOverlap_Contour] ON [RoadRegistryEditor].[ExtractRequestOverlap]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExtractRequestOverlap",
                schema: "RoadRegistryEditor",
                table: "ExtractRequestOverlap");

            migrationBuilder.AddPrimaryKey(
                    name: "PK_ExtractRequestOverlap",
                    schema: "RoadRegistryEditor",
                    table: "ExtractRequestOverlap",
                    column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.DropIndex(
                name: "IX_ExtractRequestOverlap_DownloadId1",
                schema: "RoadRegistryEditor",
                table: "ExtractRequestOverlap");

            migrationBuilder.DropIndex(
                name: "IX_ExtractRequestOverlap_DownloadId2",
                schema: "RoadRegistryEditor",
                table: "ExtractRequestOverlap");
        }
    }
}
