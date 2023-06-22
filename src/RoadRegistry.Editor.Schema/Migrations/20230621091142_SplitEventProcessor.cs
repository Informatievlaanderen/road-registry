using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class SplitEventProcessor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO [RoadRegistryEditorMeta].[ProjectionStates] ([Name], [Position])
SELECT 'roadregistry-editor-changefeed-projectionhost' as Name,[Position]
FROM [RoadRegistryEditorMeta].[ProjectionStates]
WHERE [Name] = 'roadregistry-editor-projectionhost'
UNION
SELECT 'roadregistry-editor-extractdownload-projectionhost' as Name,[Position]
FROM [RoadRegistryEditorMeta].[ProjectionStates]
WHERE [Name] = 'roadregistry-editor-projectionhost'
UNION
SELECT 'roadregistry-editor-extractrequest-projectionhost' as Name,[Position]
FROM [RoadRegistryEditorMeta].[ProjectionStates]
WHERE [Name] = 'roadregistry-editor-projectionhost'
UNION
SELECT 'roadregistry-editor-extractupload-projectionhost' as Name,[Position]
FROM [RoadRegistryEditorMeta].[ProjectionStates]
WHERE [Name] = 'roadregistry-editor-projectionhost'
UNION
SELECT 'roadregistry-editor-municipality-projectionhost' as Name,[Position]
FROM [RoadRegistryEditorMeta].[ProjectionStates]
WHERE [Name] = 'roadregistry-editor-projectionhost'
UNION
SELECT 'roadregistry-editor-organization-projectionhost' as Name,[Position]
FROM [RoadRegistryEditorMeta].[ProjectionStates]
WHERE [Name] = 'roadregistry-editor-projectionhost';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [RoadRegistryEditorMeta].[ProjectionStates] where [Name] <> 'roadregistry-editor-projectionhost';
");
        }
    }
}
