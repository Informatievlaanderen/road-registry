using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class AddExtractRequestSpatialIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExtractRequest",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");

            migrationBuilder.AddPrimaryKey(
                    name: "PK_ExtractRequest",
                    schema: "RoadRegistryEditor",
                    table: "ExtractRequest",
                    column: "DownloadId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.Sql(
                @"CREATE SPATIAL INDEX [SPATIAL_RoadRegistryEditorExtractRequest_Contour] ON [RoadRegistryEditor].[ExtractRequest] ([Contour])
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
            migrationBuilder.Sql(@"DROP INDEX [SPATIAL_RoadRegistryEditorExtractRequest_Contour] ON [RoadRegistryEditor].[ExtractRequest]");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExtractRequest",
                schema: "RoadRegistryEditor",
                table: "ExtractRequest");

            migrationBuilder.AddPrimaryKey(
                    name: "PK_ExtractRequest",
                    schema: "RoadRegistryEditor",
                    table: "ExtractRequest",
                    column: "DownloadId")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
