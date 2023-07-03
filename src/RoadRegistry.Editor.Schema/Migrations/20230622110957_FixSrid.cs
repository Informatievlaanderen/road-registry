using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadRegistry.Editor.Schema.Migrations
{
    public partial class FixSrid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [RoadRegistryEditor].[RoadNode]
SET [Geometry].STSrid = 31370
WHERE [Geometry].STSrid = 0;

UPDATE [RoadRegistryEditor].[RoadSegment]
SET [Geometry].STSrid = 31370
WHERE [Geometry].STSrid = 0;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
